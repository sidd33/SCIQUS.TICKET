namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class TicketCommentResponse
	{
		public Guid TicketCommentId { get; set; }

		public Guid TicketId { get; set; }

		public string Comment { get; set; } = null!;


		public string CreatedByUserId { get; set; } = null!;

		public string? CreatedByUserName { get; set; }


		public DateTime CreatedDate { get; set; }
	}
}