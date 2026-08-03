using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketAssignment
	{
		public Guid TicketAssignmentId { get; set; }


		public Guid TicketId { get; set; }

		public Ticket Ticket { get; set; } = null!;


		public string AssignedToUserId { get; set; } = null!;

		public ApplicationUser AssignedToUser { get; set; } = null!;


		public string AssignedByUserId { get; set; } = null!;

		public ApplicationUser AssignedByUser { get; set; } = null!;


		public DateTime AssignedDate { get; set; }

		public DateTime? UnassignedDate { get; set; }


		public string? Remarks { get; set; }


		public bool IsDeleted { get; set; }
	}
}