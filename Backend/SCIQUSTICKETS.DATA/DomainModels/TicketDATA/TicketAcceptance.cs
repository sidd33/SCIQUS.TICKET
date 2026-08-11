// SCIQUSTICKETS.DATA/DomainModels/TicketDATA/TicketAcceptance.cs
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketAcceptance
	{
		public Guid TicketAcceptanceId { get; set; } = Guid.NewGuid();

		public Guid TicketId { get; set; }
		public Ticket Ticket { get; set; } = null!;

		public string AssignedEmployeeId { get; set; } = null!;
		public Employee AssignedEmployee { get; set; } = null!;

		public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Expired

		public DateTime AssignedAt { get; set; }
		public DateTime AcceptanceDeadlineAt { get; set; }
		public DateTime? RespondedAt { get; set; }

		public string? RejectionReason { get; set; }

		public int AttemptNumber { get; set; } = 1;
	}
}