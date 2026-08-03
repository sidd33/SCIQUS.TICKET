using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class TicketBusinessImpactRepository : GenericRepository<TicketBusinessTypeImpact>, ITicketBusinessImpactRepository
    {
        public TicketBusinessImpactRepository(AppDbContext context) : base(context) { }

        public async Task<(IEnumerable<TicketBusinessTypeImpact> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize)
        {
            var query = _dbSet.Where(i => !i.IsDeleted);

            if (!includeInactive)
                query = query.Where(i => i.Status);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(i => i.Name.Contains(search));

            query = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
                "createddate" => sortDescending ? query.OrderByDescending(i => i.CreatedDate) : query.OrderBy(i => i.CreatedDate),
                _ => query.OrderBy(i => i.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await _dbSet.AnyAsync(i =>
                !i.IsDeleted &&
                i.Name.ToLower() == normalized &&
                (excludeId == null || i.TicketBusinessTypeImpactId != excludeId));
        }

        public Task<bool> IsUsedByOpenTicketsAsync(Guid ticketBusinessTypeImpactId)
        {
            // TODO(Module 2): query Tickets where TicketBusinessTypeImpactId == ticketBusinessTypeImpactId && IsOpen.
            return Task.FromResult(false);
        }
    }
}
