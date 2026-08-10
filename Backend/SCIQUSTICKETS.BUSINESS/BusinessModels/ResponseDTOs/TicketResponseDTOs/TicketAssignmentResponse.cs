namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class TicketAssignmentResponse
	{
		public Guid TicketAssignmentId { get; set; }

		public Guid TicketId { get; set; }


		public string AssignedToUserId { get; set; } = null!;

		public string? AssignedToUserName { get; set; }


		public DateTime AssignedDate { get; set; }


		public string? Remarks { get; set; }
	}
}