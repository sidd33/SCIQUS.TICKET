using System;
using System.Threading.Tasks;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface ITicketNotificationService
    {
        Task NotifyTicketCreatedAsync(Guid ticketId, string? actorUserId);
        Task NotifyAssignedAsync(Guid ticketId, string? actorUserId);
        Task NotifyTransferredAsync(Guid ticketId, string? actorUserId);
        Task NotifyCommentAddedAsync(Guid ticketId, string? actorUserId, bool isCustomerVisible);
        Task NotifyPriorityChangedAsync(Guid ticketId, string? actorUserId);
        Task NotifySlaBreachedAsync(Guid ticketId);
        Task NotifyPendingClosureAsync(Guid ticketId, string? actorUserId);
        Task NotifyClosedAsync(Guid ticketId, string? actorUserId);
        Task NotifyReopenedAsync(Guid ticketId, string? actorUserId);
    }
}
