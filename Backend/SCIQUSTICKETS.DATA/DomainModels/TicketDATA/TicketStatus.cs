namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketStatus
	{
		public Guid TicketStatusId { get; set; }

		public string Name { get; set; } = null!;

		public string? Description { get; set; }


		public bool IsClosed { get; set; }

		public bool Status { get; set; }

		public bool IsDeleted { get; set; }


		public DateTime CreatedDate { get; set; }

		public DateTime LastUpdatedDate { get; set; }


		public ICollection<Ticket> Tickets { get; set; }
			= new List<Ticket>();
	}
}