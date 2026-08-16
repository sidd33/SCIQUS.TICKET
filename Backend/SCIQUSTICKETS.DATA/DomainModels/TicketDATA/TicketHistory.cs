using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketHistory
	{
		public Guid TicketHistoryId { get; set; }


		public Guid TicketId { get; set; }

		public Ticket Ticket { get; set; } = null!;


		public Guid? OldStatusId { get; set; }

		public Guid? NewStatusId { get; set; }


		public string ChangeDescription { get; set; } = null!;


		public string ChangedByUserId { get; set; } = null!;

		public ApplicationUser ChangedByUser { get; set; } = null!;

		public string? ChangedByAccountId { get; set; }


		public DateTime CreatedDate { get; set; }
	}
}