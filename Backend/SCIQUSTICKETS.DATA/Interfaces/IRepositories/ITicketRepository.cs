using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
	public interface ITicketRepository : IGenericRepository<Ticket>
	{
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
			int pageSize,
			string? ownershipUserId = null,
			Guid? ownershipDepartmentId = null,
			string? createdByUserId = null);

		Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId);
		Task<Ticket> CreateTicketAsync(Ticket ticket);
		Task<bool> ExistsAsync(Guid ticketId);
		Task<bool> HasOpenTicketsForAccountAsync(string accountId);

		Task AddCommentAsync(TicketComment comment);
		Task AddHistoryAsync(TicketHistory history);
		Task<TicketStatus?> GetStatusByIdAsync(Guid statusId);
		Task<TicketStatus?> GetStatusByNameAsync(string name);

		Task<TicketComment?> GetCommentAsync(Guid ticketId, Guid commentId);
		void UpdateComment(TicketComment comment);
	}
}