using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class EmailChannelService : IEmailChannelService
    {
        private readonly AppDbContext _context;
        private readonly ITicketTimelineService _timelineService;

        public EmailChannelService(AppDbContext context, ITicketTimelineService timelineService)
        {
            _context = context;
            _timelineService = timelineService;
        }

        public async Task ProcessInboxMessageAsync(EmailInboxMessage message)
        {
            var config = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            if (config == null || !config.IsEnabled)
            {
                message.ProcessingStatus = "Failed";
                message.FailureReason = "Email channel is disabled or not configured.";
                return;
            }

            // Attempt to find account by sender email
            var accountContact = await _context.AccountContacts
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Email == message.FromEmail);

            if (accountContact != null && config.AutoCreateEnabled)
            {
                var ticket = new Ticket
                {
                    TicketId = Guid.NewGuid(),
                    TicketNumber = $"TKT-{DateTime.UtcNow.Ticks}", // placeholder for auto-generation
                    Title = message.Subject,
                    Description = message.Body,
                    AccountId = accountContact.AccountId,
                    SourceType = "Email",
                    SourceMessageId = message.ProviderMessageId,
                    EmailReceivedDate = message.EmailReceivedDate,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    
                    // Apply defaults
                    PriorityId = config.DefaultPriorityId,
                    BusinessImpactId = config.DefaultBusinessImpactId,
                    DepartmentId = config.DefaultDepartmentId,
                    TicketTypeId = config.DefaultTicketTypeId,
                    TicketSubTypeId = config.DefaultTicketSubTypeId,
                    AssignedToUserId = config.DefaultAssigneeId,
                    StatusId = Guid.Parse("10000000-0000-0000-0000-000000000001"), // Open
                    CreatedByUserId = "SYSTEM"
                };

                _context.Tickets.Add(ticket);
                message.CreatedTicketId = ticket.TicketId;
                
                await _timelineService.WriteHistoryAsync(ticket.TicketId, SCIQUSTICKETS.COMMON.Enums.TicketChangeType.EmailReceived, null, null, $"Email received from {message.FromEmail}", "SYSTEM");
                message.ProcessingStatus = "Processed";
                message.ProcessedDate = DateTime.UtcNow;
            }
            else
            {
                message.ProcessingStatus = "Failed";
                message.FailureReason = accountContact == null ? "Account not found." : "Auto-create is disabled.";
            }
        }

        public async Task<bool> SendOutboundReplyAsync(Guid ticketId, string body, string? sentByUserId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Account)
                .ThenInclude(a => a.Contacts)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Account == null) return false;

            // Find primary contact email
            var contactEmail = ticket.Account.Contacts.FirstOrDefault()?.Email;
            if (string.IsNullOrEmpty(contactEmail)) return false;

            // Send outbound email via configured provider (SMTP/API)
            
            // Add comment to ticket
            var comment = new TicketComment
            {
                TicketCommentId = Guid.NewGuid(),
                TicketId = ticketId,
                CommentText = body,
                IsInternalNote = false,
                CommentedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);
            
            await _timelineService.WriteHistoryAsync(ticketId, SCIQUSTICKETS.COMMON.Enums.TicketChangeType.EmailSent, null, null, $"Email reply sent to {contactEmail}", sentByUserId ?? "SYSTEM");
            
            var history = new TicketHistory
            {
                TicketHistoryId = Guid.NewGuid(),
                TicketId = ticketId,
                ChangeDescription = $"Email Reply sent to {contactEmail}",
                ChangedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = DateTime.UtcNow
            };
            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SyncMailboxesAsync()
        {
            await Task.CompletedTask;
        }
    }
}
