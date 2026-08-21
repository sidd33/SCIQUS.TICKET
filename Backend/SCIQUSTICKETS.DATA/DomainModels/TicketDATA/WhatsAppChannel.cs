using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.COMMON.Helpers;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
    public enum WhatsAppProviderType
    {
        MetaCloudAPI,
        Twilio,
        Gupshup
    }

    public class WhatsAppChannelConfig
    {
        public Guid WhatsAppChannelConfigId { get; set; }
        
        public WhatsAppProviderType Provider { get; set; }
        public string BusinessPhoneNumberId { get; set; } = null!;
        public string EncryptedApiToken { get; set; } = null!;
        public string WebhookVerifyToken { get; set; } = null!;
        public string AppSecret { get; set; } = null!;
        
        public bool IsEnabled { get; set; }
        public bool AutoCreateEnabled { get; set; }
        
        public bool IsAuthValid { get; set; } = true;
        public string? LastError { get; set; }

        // Default ticket values when auto-creating
        public Guid DefaultPriorityId { get; set; }
        public TicketPriority DefaultPriority { get; set; } = null!;

        public Guid DefaultBusinessImpactId { get; set; }
        public TicketBusinessTypeImpact DefaultBusinessImpact { get; set; } = null!;

        public Guid DefaultDepartmentId { get; set; }
        public Department DefaultDepartment { get; set; } = null!;

        public Guid DefaultTicketTypeId { get; set; }
        public TicketType DefaultTicketType { get; set; } = null!;

        public Guid DefaultTicketSubTypeId { get; set; }
        public TicketSubType DefaultTicketSubType { get; set; } = null!;

        public string? DefaultAssigneeId { get; set; }
    }

    public class WhatsAppInboxMessage
    {
        public Guid WhatsAppInboxMessageId { get; set; }
        public string ProviderMessageId { get; set; } = null!;
        
        public string FromPhone { get; set; } = null!; // E.164
        public string? FromName { get; set; }
        public string MessageType { get; set; } = "text"; // text/image/document
        public string? Body { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime ReceivedDate { get; set; }
        
        public string ProcessingStatus { get; set; } = "Pending"; 
        public string? FailureReason { get; set; }

        public Guid? CreatedTicketId { get; set; }
        public Ticket? CreatedTicket { get; set; }
        
        public DateTime CreatedDate { get; set; } = TimeHelper.GetIndianTime();
        public DateTime? ProcessedDate { get; set; }
    }

    public class WhatsAppOutboundMessage
    {
        public Guid WhatsAppOutboundMessageId { get; set; }
        
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public string ToPhone { get; set; } = null!; // E.164
        public string? Body { get; set; }
        public string? TemplateName { get; set; }
        
        public string? ProviderMessageId { get; set; }
        
        // Queued, Sent, Delivered, Failed
        public string Status { get; set; } = "Queued";
        
        public string? SentByUserId { get; set; }
        public ApplicationUser? SentByUser { get; set; }
        
        public DateTime SentDate { get; set; } = TimeHelper.GetIndianTime();
        public string? FailureReason { get; set; }
    }
}
