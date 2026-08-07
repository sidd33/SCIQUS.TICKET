namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class TicketHistoryResponse
	{
		public Guid TicketHistoryId { get; set; }

		public Guid TicketId { get; set; }


		public string Action { get; set; } = null!;

		public string? OldValue { get; set; }

		public string? NewValue { get; set; }


		public string ChangedByUserId { get; set; } = null!;

		public string? ChangedByUserName { get; set; }


		public DateTime ChangedDate { get; set; }
	}
}