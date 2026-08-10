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

        public EmailChannelService(AppDbContext context)
        {
            _context = context;
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

            // Threading logic
            Ticket? matchingTicket = null;

            // 1. Check if subject or body contains a ticket number
            // (Placeholder: simplified lookup for illustration, typically use regex)
            // 2. Check SourceMessageId (e.g. In-Reply-To header matched)
            
            // Assuming no match for now, try to find account
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

            // Placeholder: Call actual SMTP/Email provider API to send email to contactEmail
            // ...
            
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
            
            // Add history
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
            // Placeholder: Called by background job to fetch from Provider
            await Task.CompletedTask;
        }
    }
}
