// SCIQUSTICKETS.BUSINESS/Interfaces/IService/IAcceptanceService.cs
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IAcceptanceService
	{
		/// <summary>
		/// Writes the first (or a subsequent fallback) Pending TicketAcceptance row for a
		/// newly-assigned ticket. Called by TicketService right after an assignee is resolved,
		/// when the sub-type/global config requires acceptance.
		/// </summary>
		Task StartAcceptanceCycleAsync(Ticket ticket, Employee employee, int attemptNumber);

		Task<bool> AcceptAsync(Guid ticketId, string employeeId);
		Task<bool> RejectAsync(Guid ticketId, string employeeId, string reason);
		Task ProcessExpiredAsync();
	}
}