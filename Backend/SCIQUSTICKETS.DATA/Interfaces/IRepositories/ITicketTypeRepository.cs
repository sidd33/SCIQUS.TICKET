using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface ITicketTypeRepository : IGenericRepository<TicketType>
    {
        Task<(IEnumerable<TicketType> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize);

        Task<TicketType?> GetByIdWithSubTypesAsync(Guid ticketTypeId);

        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

        Task<int> GetActiveSubTypeCountAsync(Guid ticketTypeId);

        // TODO(Module 2): once the Ticket table exists, wire this to check
        // for open tickets referencing this type before allowing deactivate/delete.
        Task<bool> IsUsedByOpenTicketsAsync(Guid ticketTypeId);
    }
}
