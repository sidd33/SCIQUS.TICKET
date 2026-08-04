using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class TicketTypeRepository : GenericRepository<TicketType>, ITicketTypeRepository
    {
        public TicketTypeRepository(AppDbContext context) : base(context) { }

        public async Task<(IEnumerable<TicketType> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize)
        {
            var query = _dbSet.Where(t => !t.IsDeleted);

            if (!includeInactive)
                query = query.Where(t => t.Status);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Name.Contains(search));

            query = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "createddate" => sortDescending ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate),
                _ => query.OrderBy(t => t.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TicketType?> GetByIdWithSubTypesAsync(Guid ticketTypeId)
        {
            return await _dbSet
                .Include(t => t.TicketSubTypes.Where(st => !st.IsDeleted))
                .FirstOrDefaultAsync(t => t.TicketTypeId == ticketTypeId && !t.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await _dbSet.AnyAsync(t =>
                !t.IsDeleted &&
                t.Name.ToLower() == normalized &&
                (excludeId == null || t.TicketTypeId != excludeId));
        }

        public async Task<int> GetActiveSubTypeCountAsync(Guid ticketTypeId)
        {
            return await _context.Set<TicketSubType>()
                .CountAsync(st => st.TicketTypeId == ticketTypeId && st.Status && !st.IsDeleted);
        }

		public async Task<bool> IsUsedByOpenTicketsAsync(Guid ticketTypeId)
		{
			return await _context.Set<Ticket>()
				.AnyAsync(t => t.TicketTypeId == ticketTypeId && t.IsOpen && !t.IsDeleted);
		}
	}
}
