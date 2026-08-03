using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface ITicketPriorityRepository : IGenericRepository<TicketPriority>
    {
        Task<(IEnumerable<TicketPriority> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize);

        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

        // TODO(Module 2): wire once the Ticket table exists.
        Task<bool> IsUsedByOpenTicketsAsync(Guid ticketPriorityId);
    }
}
