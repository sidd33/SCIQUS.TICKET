namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class TicketResponse
	{
		public Guid TicketId { get; set; }

		public string TicketNumber { get; set; } = null!;

		public string Title { get; set; } = null!;

		public string Description { get; set; } = null!;


		// CRM
		public string? AccountId { get; set; }

		public string? AccountName { get; set; }


		// Internal ticket
		public string? RaisedByEmployeeId { get; set; }

		public string? RaisedByEmployeeName { get; set; }

		public bool IsInternal { get; set; }

		public string SourceType { get; set; } = null!;

		// Department
		public Guid DepartmentId { get; set; }
		public string? DepartmentName { get; set; }

		// Master data
		public Guid TicketTypeId { get; set; }

		public string TicketTypeName { get; set; } = null!;


		public Guid TicketSubTypeId { get; set; }

		public string TicketSubTypeName { get; set; } = null!;


		public Guid PriorityId { get; set; }

		public string PriorityName { get; set; } = null!;
		public int PriorityLevel { get; set; }


		public Guid BusinessImpactId { get; set; }

		public string BusinessImpactName { get; set; } = null!;


		// Status
		public Guid StatusId { get; set; }

		public string StatusName { get; set; } = null!;

		public bool IsClosed { get; set; }


		// Assignment
		public string? AssignedToUserId { get; set; }

		public string? AssignedToUserName { get; set; }

		// SLA & Acceptance
		public DateTime? SlaDueDate { get; set; }
		public string? SlaMetStatus { get; set; }
		public string? AcceptanceStatus { get; set; }
		public DateTime? AcceptanceDeadlineAt { get; set; }


		// Workflow helper
		public bool IsOpen { get; set; }


		// Audit
		public string CreatedByUserId { get; set; } = null!;

		public string? CreatedByUserName { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime LastUpdatedDate { get; set; }

		public bool IsAwaitingAcceptance { get; set; }
	}
}