using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class TicketPriorityRepository : GenericRepository<TicketPriority>, ITicketPriorityRepository
    {
        public TicketPriorityRepository(AppDbContext context) : base(context) { }

        public async Task<(IEnumerable<TicketPriority> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize)
        {
            var query = _dbSet.Where(p => !p.IsDeleted);

            if (!includeInactive)
                query = query.Where(p => p.Status);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));

            query = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "level" => sortDescending ? query.OrderByDescending(p => p.Level) : query.OrderBy(p => p.Level),
                "slainhours" => sortDescending ? query.OrderByDescending(p => p.SlaInHours) : query.OrderBy(p => p.SlaInHours),
                _ => query.OrderBy(p => p.Level)
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
            return await _dbSet.AnyAsync(p =>
                !p.IsDeleted &&
                p.Name.ToLower() == normalized &&
                (excludeId == null || p.TicketPriorityId != excludeId));
        }

		public async Task<bool> IsUsedByOpenTicketsAsync(Guid ticketPriorityId)
		{
			return await _context.Set<Ticket>()
				.AnyAsync(t => t.PriorityId == ticketPriorityId && t.IsOpen && !t.IsDeleted);
		}
	}
}
