using System;
using System.Collections.Generic;
using System.Text;

using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
	public interface ITicketRepository : IGenericRepository<Ticket>
	{
		// Listing with filters + paging
		Task<(IEnumerable<Ticket> Items, int TotalCount)> GetAllPagedAsync(
			string? search,
			Guid? ticketTypeId,
			Guid? ticketSubTypeId,
			Guid? priorityId,
			Guid? businessImpactId,
			Guid? statusId,
			string? accountId,
			string? assignedToUserId,
			bool? isInternal,
			bool? isOpen,
			DateTime? fromDate,
			DateTime? toDate,
			string? sortBy,
			bool sortDescending,
			int page,
			int pageSize);


		// Detail screen
		Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId);


		// Ticket number generation
		Task<string> GenerateTicketNumberAsync();


		// Validation helpers
		Task<bool> ExistsAsync(Guid ticketId);


		// Used for delete/status rules
		Task<bool> HasOpenTicketsForAccountAsync(string accountId);
	}
}
