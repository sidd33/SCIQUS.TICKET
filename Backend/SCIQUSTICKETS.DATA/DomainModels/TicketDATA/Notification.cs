using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
    public class Notification
    {
        public Guid NotificationId { get; set; }
        
        public string EventType { get; set; } = null!;
        public string EntityType { get; set; } = "Ticket";
        public string? EntityId { get; set; }
        public string? RedirectUrl { get; set; }
        
        public string? ActorUserId { get; set; }
        public ApplicationUser? ActorUser { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<NotificationUser> NotificationUsers { get; set; } = new List<NotificationUser>();
        public NotificationData? NotificationData { get; set; }
    }

    public class NotificationUser
    {
        public Guid NotificationUserId { get; set; }
        
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    public class NotificationData
    {
        public Guid NotificationDataId { get; set; }
        
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;

        public string PayloadJson { get; set; } = null!; // Stored structured payload
    }
}
