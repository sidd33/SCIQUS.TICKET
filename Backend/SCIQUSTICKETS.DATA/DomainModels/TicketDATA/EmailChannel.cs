using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
    public class EmailTicketConfig
    {
        public Guid EmailTicketConfigId { get; set; }
        
        public string Provider { get; set; } = "Google"; // "Google" or "Outlook"
        public string EmailAddress { get; set; } = null!;
        public string EncryptedAccessToken { get; set; } = null!;
        public string EncryptedRefreshToken { get; set; } = null!;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? AppPassword { get; set; }
        
        public bool IsEnabled { get; set; }
        public bool AutoCreateEnabled { get; set; }
        
        public int PollingIntervalMinutes { get; set; } = 15;
        public bool IsAuthValid { get; set; } = true;
        
        public DateTime? LastPollDate { get; set; }
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

    public class EmailInboxMessage
    {
        public Guid EmailInboxMessageId { get; set; }
        public string ProviderMessageId { get; set; } = null!;
        
        public string FromEmail { get; set; } = null!;
        public string? FromName { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime EmailReceivedDate { get; set; }
        
        // e.g., "Pending", "Processed", "Failed"
        public string ProcessingStatus { get; set; } = "Pending"; 
        public string? FailureReason { get; set; }

        public Guid? CreatedTicketId { get; set; }
        public Ticket? CreatedTicket { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedDate { get; set; }
    }
}
