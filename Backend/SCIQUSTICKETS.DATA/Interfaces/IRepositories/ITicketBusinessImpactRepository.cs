using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface ITicketBusinessImpactRepository : IGenericRepository<TicketBusinessTypeImpact>
    {
        Task<(IEnumerable<TicketBusinessTypeImpact> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize);

        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

        // TODO(Module 2): wire once the Ticket table exists.
        Task<bool> IsUsedByOpenTicketsAsync(Guid ticketBusinessTypeImpactId);
    }
}
