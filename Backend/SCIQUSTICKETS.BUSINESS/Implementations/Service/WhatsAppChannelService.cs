using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class WhatsAppChannelService : IWhatsAppChannelService
    {
        private readonly AppDbContext _context;

        public WhatsAppChannelService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ProcessInboxMessageAsync(WhatsAppInboxMessage message)
        {
            var config = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (config == null || !config.IsEnabled)
            {
                message.ProcessingStatus = "Failed";
                message.FailureReason = "WhatsApp channel is disabled or not configured.";
                return;
            }

            // Normalise to E.164 (simplified)
            var phone = message.FromPhone.Trim();
            if (!phone.StartsWith("+")) phone = "+" + phone;

            // Find account
            var accountContact = await _context.AccountContacts
                .FirstOrDefaultAsync(c => c.MobileNumber == phone || c.AlternateMobileNumber == phone);

            if (accountContact != null && config.AutoCreateEnabled)
            {
                var ticket = new Ticket
                {
                    TicketId = Guid.NewGuid(),
                    TicketNumber = $"TKT-{DateTime.UtcNow.Ticks}",
                    Title = "WhatsApp Message from " + phone,
                    Description = message.Body ?? "[Media]",
                    AccountId = accountContact.AccountId,
                    SourceType = "WhatsApp",
                    SourceMessageId = message.ProviderMessageId,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    
                    PriorityId = config.DefaultPriorityId,
                    BusinessImpactId = config.DefaultBusinessImpactId,
                    DepartmentId = config.DefaultDepartmentId,
                    TicketTypeId = config.DefaultTicketTypeId,
                    TicketSubTypeId = config.DefaultTicketSubTypeId,
                    AssignedToUserId = config.DefaultAssigneeId,
                    StatusId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
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

        public async Task<bool> SendOutboundReplyAsync(Guid ticketId, string body, string? templateName, string? sentByUserId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Account)
                .ThenInclude(a => a.Contacts)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Account == null) return false;

            var contactPhone = ticket.Account.Contacts.FirstOrDefault()?.MobileNumber;
            if (string.IsNullOrEmpty(contactPhone)) return false;

            // Placeholder: Call actual WhatsApp provider API
            // ...
            
            var outbound = new WhatsAppOutboundMessage
            {
                WhatsAppOutboundMessageId = Guid.NewGuid(),
                TicketId = ticketId,
                ToPhone = contactPhone,
                Body = body,
                TemplateName = templateName,
                Status = "Sent",
                SentByUserId = sentByUserId,
                SentDate = DateTime.UtcNow
            };
            _context.WhatsAppOutboundMessages.Add(outbound);

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
            
            var history = new TicketHistory
            {
                TicketHistoryId = Guid.NewGuid(),
                TicketId = ticketId,
                ChangeDescription = $"WhatsApp Reply sent to {contactPhone}",
                ChangedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = DateTime.UtcNow
            };
            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();
            return true;
        }

        public bool ValidateWebhookSignature(string payload, string signature, string providerSecret)
        {
            // Placeholder: standard HMAC-SHA256 signature verification
            // byte[] keyBytes = Encoding.UTF8.GetBytes(providerSecret);
            // using var hmac = new HMACSHA256(keyBytes);
            // byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            // string expectedSignature = "sha256=" + BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            // return signature == expectedSignature;
            return true; 
        }
    }
}
