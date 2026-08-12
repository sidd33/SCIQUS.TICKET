// SCIQUSTICKETS.BUSINESS/BusinessModels/ResponseDTOs/TicketResponseDTOs/PortalTicketResponses.cs
namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	// Slim, portal-safe shape — intentionally omits internal-only fields
	// (AssignedTo*, AcceptanceStatus, RaisedByEmployee*, CreatedByUser*, Department).
	public class PortalTicketResponse
	{
		public Guid TicketId { get; set; }
		public string TicketNumber { get; set; } = null!;
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;

		public string TicketTypeName { get; set; } = null!;
		public string TicketSubTypeName { get; set; } = null!;
		public string PriorityName { get; set; } = null!;
		public string BusinessImpactName { get; set; } = null!;

		public string StatusName { get; set; } = null!;
		public bool IsOpen { get; set; }

		public DateTime? SlaDueDate { get; set; }
		public string? SlaMetStatus { get; set; }

		public DateTime CreatedDate { get; set; }
		public DateTime LastUpdatedDate { get; set; }
	}

	public class PortalTicketDetailResponse : PortalTicketResponse
	{
		public List<TicketCommentResponse> Comments { get; set; } = new();
		public List<TicketAttachmentResponse> Attachments { get; set; } = new();
	}
}