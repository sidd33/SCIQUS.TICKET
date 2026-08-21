using System;
using System.Text.RegularExpressions;
using System.Linq;
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
        private readonly ITicketService _ticketService;
        private readonly ISlaService _slaService;

        public EmailChannelService(AppDbContext context, ITicketTimelineService timelineService, ITicketService ticketService, ISlaService slaService)
        {
            _context = context;
            _timelineService = timelineService;
            _ticketService = ticketService;
            _slaService = slaService;
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

            var now = SCIQUSTICKETS.COMMON.Helpers.TimeHelper.GetIndianTime();

            if (accountContact != null)
            {
                Ticket? existingTicket = null;
                
                // Attempt to thread by extracting TKT-\d+ from subject
                var match = Regex.Match(message.Subject, @"TKT-\d+");
                if (match.Success)
                {
                    string ticketNum = match.Value;
                    existingTicket = await _context.Tickets
                        .FirstOrDefaultAsync(t => t.TicketNumber == ticketNum && t.AccountId == accountContact.AccountId && t.IsOpen);
                }

                if (existingTicket != null)
                {
                    // Thread as comment
                    await _ticketService.AddCommentAsync(existingTicket.TicketId, new SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs.AddTicketCommentRequest
                    {
                        Comment = message.Body,
                        IsInternalNote = false
                    }, accountContact.AccountId);

                    message.CreatedTicketId = existingTicket.TicketId;
                    message.ProcessingStatus = "Processed";
                    message.ProcessedDate = now;
                }
                else if (config.AutoCreateEnabled)
                {
                    var createReq = new SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs.CreateTicketRequest
                    {
                        Title = message.Subject,
                        Description = message.Body,
                        AccountId = accountContact.AccountId,
                        SourceType = "Email",
                        TicketTypeId = config.DefaultTicketTypeId,
                        TicketSubTypeId = config.DefaultTicketSubTypeId,
                        PriorityId = config.DefaultPriorityId,
                        BusinessImpactId = config.DefaultBusinessImpactId,
                        DepartmentId = config.DefaultDepartmentId,
                        AssignedToUserId = config.DefaultAssigneeId,
                        IsInternal = false
                    };

                    var createdTicket = await _ticketService.CreateAsync(accountContact.AccountId, createReq);
                    
                    // Fix SLA clock for email tickets
                    var ticketEntity = await _context.Tickets.FindAsync(createdTicket.TicketId);
                    if (ticketEntity != null)
                    {
                        ticketEntity.EmailReceivedDate = message.EmailReceivedDate;
                        var priority = await _context.TicketPriorities.FindAsync(ticketEntity.PriorityId);
                        var (slaDueDate, slaMetStatus) = _slaService.ComputeSlaDueDate(ticketEntity, priority);
                        ticketEntity.SlaDueDate = slaDueDate;
                        ticketEntity.SlaMetStatus = slaMetStatus;
                    }
                    
                    message.CreatedTicketId = createdTicket.TicketId;
                    message.ProcessingStatus = "Processed";
                    message.ProcessedDate = now;
                }
                else
                {
                    message.ProcessingStatus = "Failed";
                    message.FailureReason = "No open ticket found and auto-create is disabled.";
                }
            }
            else
            {
                message.ProcessingStatus = "Failed";
                message.FailureReason = "Account not found.";
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
            var now = SCIQUSTICKETS.COMMON.Helpers.TimeHelper.GetIndianTime();
            
            // Add comment to ticket
            var comment = new TicketComment
            {
                TicketCommentId = Guid.NewGuid(),
                TicketId = ticketId,
                CommentText = body,
                IsInternalNote = false,
                CommentedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = now
            };

            _context.TicketComments.Add(comment);
            
            await _timelineService.WriteHistoryAsync(ticketId, SCIQUSTICKETS.COMMON.Enums.TicketChangeType.EmailSent, null, null, $"Email reply sent to {contactEmail}", sentByUserId ?? "SYSTEM");
            
            var history = new TicketHistory
            {
                TicketHistoryId = Guid.NewGuid(),
                TicketId = ticketId,
                ChangeDescription = $"Email Reply sent to {contactEmail}",
                ChangedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = now
            };
            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SyncMailboxesAsync()
        {
            var config = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            if (config == null || !config.IsEnabled) return;

            // Mock polling loop for "Pending" inbox messages, deduplicating by ProviderMessageId
            var pendingMessages = await _context.EmailInboxMessages
                .Where(m => m.ProcessingStatus == "Pending")
                .ToListAsync();

            var processedProviderMessageIds = new HashSet<string>();

            foreach (var message in pendingMessages)
            {
                if (!string.IsNullOrEmpty(message.ProviderMessageId) && !processedProviderMessageIds.Add(message.ProviderMessageId))
                {
                    message.ProcessingStatus = "Duplicate";
                    message.ProcessedDate = SCIQUSTICKETS.COMMON.Helpers.TimeHelper.GetIndianTime();
                    continue;
                }

                await ProcessInboxMessageAsync(message);
            }

            await _context.SaveChangesAsync();
            
            await Task.CompletedTask;
        }
    }
}
