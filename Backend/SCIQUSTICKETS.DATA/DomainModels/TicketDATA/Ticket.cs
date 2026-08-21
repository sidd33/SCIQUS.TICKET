using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class Ticket
	{
		public Guid TicketId { get; set; }

		public string TicketNumber { get; set; } = null!;

		public string Title { get; set; } = null!;

		public string Description { get; set; } = null!;


		// CRM
		public string? AccountId { get; set; }
		public Account? Account { get; set; }


		// Internal ticket creator
		public string? RaisedByEmployeeId { get; set; }
		public Employee? RaisedByEmployee { get; set; }

		public bool IsInternal { get; set; }

		// Portal / Email / WhatsApp / Internal
		public string SourceType { get; set; } = "Portal";
		
		public string? SourceMessageId { get; set; }
		public DateTime? EmailReceivedDate { get; set; }

		// Derived workflow helper
		public bool IsOpen { get; set; } = true;


		// Department
		public Guid DepartmentId { get; set; }
		public Department Department { get; set; } = null!;

		// Ticket Master
		public Guid TicketTypeId { get; set; }
		public TicketType TicketType { get; set; } = null!;


		public Guid TicketSubTypeId { get; set; }
		public TicketSubType TicketSubType { get; set; } = null!;


		public Guid PriorityId { get; set; }
		public TicketPriority Priority { get; set; } = null!;


		public Guid BusinessImpactId { get; set; }
		public TicketBusinessTypeImpact BusinessImpact { get; set; } = null!;


		// Workflow
		public Guid StatusId { get; set; }
		public TicketStatus Status { get; set; } = null!;


		// Users
		public string CreatedByUserId { get; set; } = null!;

		public ApplicationUser CreatedByUser { get; set; } = null!;


		public string? AssignedToUserId { get; set; }


		public ApplicationUser? AssignedToUser { get; set; }


		// Audit
		public DateTime CreatedDate { get; set; }

		public DateTime LastUpdatedDate { get; set; }

		public bool IsDeleted { get; set; }

		public DateTime? PendingClosureDate { get; set; }
		public string? ClosureConfirmedBy { get; set; } // "Customer" / "Auto" / "Agent"

		// SLA Fields
		public DateTime? SlaDueDate { get; set; }
		public string? SlaMetStatus { get; set; }
		public bool? IsSlaBreached { get; set; }
		public double? ResolutionTimeInHours { get; set; }
		public double? OverdueHours { get; set; }

		// Module 12 Acceptance Fields
		public string? AcceptanceStatus { get; set; } // Pending, Accepted, Rejected, Expired
		public DateTime? AcceptanceDeadlineAt { get; set; }
		public int? AcceptanceDeadlineHours { get; set; }
		public int CurrentFallbackAttempt { get; set; } = 1;

		public DateTime? ClosedDate { get; set; }
		public int ReopenCount { get; set; } = 0;
		public DateTime? LastReopenedDate { get; set; }
		public Guid? ParentTicketId { get; set; }
		public Ticket? ParentTicket { get; set; }

		// Navigation
		public ICollection<TicketAssignment> Assignments { get; set; }
			= new List<TicketAssignment>();

		public ICollection<TicketComment> Comments { get; set; }
			= new List<TicketComment>();

		public ICollection<TicketHistory> History { get; set; }
			= new List<TicketHistory>();

		public ICollection<TicketAttachment> Attachments { get; set; }
			= new List<TicketAttachment>();
	}
}