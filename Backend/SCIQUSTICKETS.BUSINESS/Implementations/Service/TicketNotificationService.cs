using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

using SCIQUSTICKETS.COMMON.Helpers;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class TicketNotificationService : ITicketNotificationService
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public TicketNotificationService(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        private async Task CreateNotificationAsync(
            string eventType,
            Guid ticketId,
            string? actorUserId,
            IEnumerable<string> potentialUserIds,
            object payload = null)
        {
            var validUserIds = potentialUserIds
                .Where(id => !string.IsNullOrEmpty(id) && id != actorUserId)
                .Distinct()
                .ToList();

            if (!validUserIds.Any()) return;

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                EventType = eventType,
                EntityType = "Ticket",
                EntityId = ticketId.ToString(),
                RedirectUrl = $"/tickets/{ticketId}",
                ActorUserId = actorUserId,
                CreatedDate = TimeHelper.GetIndianTime()
            };

            if (payload != null)
            {
                notification.NotificationData = new NotificationData
                {
                    NotificationDataId = Guid.NewGuid(),
                    NotificationId = notification.NotificationId,
                    PayloadJson = JsonSerializer.Serialize(payload)
                };
            }

            foreach (var userId in validUserIds)
            {
                notification.NotificationUsers.Add(new NotificationUser
                {
                    NotificationUserId = Guid.NewGuid(),
                    NotificationId = notification.NotificationId,
                    UserId = userId,
                    IsRead = false
                });
            }

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private async Task<Ticket?> GetTicketDetailsAsync(Guid ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Department)
                .Include(t => t.Account)
                .ThenInclude(a => a.Contacts)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        private async Task SendExternalNotificationAsync(Ticket ticket, string message, string? actorUserId)
        {
            if (ticket.SourceType == "Email")
            {
                var emailService = (IEmailChannelService?)_serviceProvider.GetService(typeof(IEmailChannelService));
                if (emailService != null) await emailService.SendOutboundReplyAsync(ticket.TicketId, message, actorUserId);
            }
            else if (ticket.SourceType == "WhatsApp")
            {
                var waService = (IWhatsAppChannelService?)_serviceProvider.GetService(typeof(IWhatsAppChannelService));
                if (waService != null) await waService.SendOutboundReplyAsync(ticket.TicketId, message, null, actorUserId);
            }
        }

        public async Task NotifyTicketCreatedAsync(Guid ticketId, string? actorUserId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null) return;

            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
                recipients.Add(ticket.AssignedToUserId);
            if (!string.IsNullOrEmpty(ticket.Department?.DepartmentHeadId))
                recipients.Add(ticket.Department.DepartmentHeadId);

            await CreateNotificationAsync("Created", ticketId, actorUserId, recipients, new { Title = ticket.Title });
            await SendExternalNotificationAsync(ticket, $"Your ticket [TKT-{ticket.TicketNumber}] has been successfully created and assigned.", actorUserId);
        }

        public async Task NotifyAssignedAsync(Guid ticketId, string? actorUserId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null || string.IsNullOrEmpty(ticket.AssignedToUserId)) return;

            await CreateNotificationAsync("Assigned", ticketId, actorUserId, new[] { ticket.AssignedToUserId }, new { Title = ticket.Title });
        }

        public async Task NotifyTransferredAsync(Guid ticketId, string? actorUserId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null) return;

            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
                recipients.Add(ticket.AssignedToUserId);
            if (!string.IsNullOrEmpty(ticket.Department?.DepartmentHeadId))
                recipients.Add(ticket.Department.DepartmentHeadId);

            await CreateNotificationAsync("Transferred", ticketId, actorUserId, recipients, new { Department = ticket.Department?.Name });
        }

        public async Task NotifyCommentAddedAsync(Guid ticketId, string? actorUserId, bool isCustomerVisible)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null) return;

            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
                recipients.Add(ticket.AssignedToUserId);
            
            if (isCustomerVisible && !string.IsNullOrEmpty(ticket.CreatedByUserId))
            {
                recipients.Add(ticket.CreatedByUserId);
            }

            await CreateNotificationAsync("CommentAdded", ticketId, actorUserId, recipients, new { IsCustomerVisible = isCustomerVisible });
        }

        public async Task NotifyPriorityChangedAsync(Guid ticketId, string? actorUserId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null || string.IsNullOrEmpty(ticket.AssignedToUserId)) return;

            await CreateNotificationAsync("PriorityChanged", ticketId, actorUserId, new[] { ticket.AssignedToUserId });
        }

        public async Task NotifySlaBreachedAsync(Guid ticketId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null) return;

            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
                recipients.Add(ticket.AssignedToUserId);
            if (!string.IsNullOrEmpty(ticket.Department?.DepartmentHeadId))
                recipients.Add(ticket.Department.DepartmentHeadId);

            await CreateNotificationAsync("SlaBreached", ticketId, null, recipients);
        }

        public async Task NotifySlaWarningAsync(Guid ticketId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null) return;

            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
                recipients.Add(ticket.AssignedToUserId);
            
            // Warnings can also go to department heads if needed, let's include them.
            if (!string.IsNullOrEmpty(ticket.Department?.DepartmentHeadId))
                recipients.Add(ticket.Department.DepartmentHeadId);

            await CreateNotificationAsync("SlaWarning", ticketId, null, recipients);
        }

        public async Task NotifyPendingClosureAsync(Guid ticketId, string? actorUserId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null || string.IsNullOrEmpty(ticket.AssignedToUserId)) return;

            await CreateNotificationAsync("PendingClosure", ticketId, actorUserId, new[] { ticket.AssignedToUserId });
        }

        public async Task NotifyClosedAsync(Guid ticketId, string? actorUserId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null) return;

            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
                recipients.Add(ticket.AssignedToUserId);

            if (!string.IsNullOrEmpty(ticket.CreatedByUserId))
            {
                recipients.Add(ticket.CreatedByUserId);
            }

            await CreateNotificationAsync("Closed", ticketId, actorUserId, recipients);
            await SendExternalNotificationAsync(ticket, $"Your ticket [TKT-{ticket.TicketNumber}] has been successfully closed. Thank you!", actorUserId);
        }

        public async Task NotifyReopenedAsync(Guid ticketId, string? actorUserId)
        {
            var ticket = await GetTicketDetailsAsync(ticketId);
            if (ticket == null) return;

            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
                recipients.Add(ticket.AssignedToUserId);
            if (!string.IsNullOrEmpty(ticket.Department?.DepartmentHeadId))
                recipients.Add(ticket.Department.DepartmentHeadId);

            await CreateNotificationAsync("Reopened", ticketId, actorUserId, recipients);
        }
    }
}
