using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class TicketSubTypeRepository : GenericRepository<TicketSubType>, ITicketSubTypeRepository
    {
        public TicketSubTypeRepository(AppDbContext context) : base(context) { }

        public async Task<(IEnumerable<TicketSubType> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            Guid? ticketTypeId,
            Guid? departmentId,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize)
        {
            var query = _dbSet
                .Include(st => st.TicketType)
                .Include(st => st.Department)
                .Include(st => st.DefaultUser)
                .Where(st => !st.IsDeleted);

            if (!includeInactive)
                query = query.Where(st => st.Status);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(st => st.Name.Contains(search));

            if (ticketTypeId.HasValue)
                query = query.Where(st => st.TicketTypeId == ticketTypeId.Value);

            if (departmentId.HasValue)
                query = query.Where(st => st.DepartmentId == departmentId.Value);

            query = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(st => st.Name) : query.OrderBy(st => st.Name),
                "createddate" => sortDescending ? query.OrderByDescending(st => st.CreatedDate) : query.OrderBy(st => st.CreatedDate),
                _ => query.OrderBy(st => st.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TicketSubType?> GetByIdWithDetailsAsync(Guid ticketSubTypeId)
        {
            return await _dbSet
                .Include(st => st.TicketType)
                .Include(st => st.Department)
                .Include(st => st.DefaultUser)
                .FirstOrDefaultAsync(st => st.TicketSubTypeId == ticketSubTypeId && !st.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await _dbSet.AnyAsync(st =>
                !st.IsDeleted &&
                st.Name.ToLower() == normalized &&
                (excludeId == null || st.TicketSubTypeId != excludeId));
        }

        public Task<bool> IsUsedByOpenTicketsAsync(Guid ticketSubTypeId)
        {
            // TODO(Module 2): query Tickets where TicketSubTypeId == ticketSubTypeId && IsOpen.
            return Task.FromResult(false);
        }

        public async Task<bool> IsTicketTypeActiveAsync(Guid ticketTypeId)
        {
            return await _context.Set<TicketType>()
                .AnyAsync(t => t.TicketTypeId == ticketTypeId && t.Status && !t.IsDeleted);
        }

        public async Task<bool> DepartmentExistsAsync(Guid departmentId)
        {
            return await _context.Set<Department>()
                .AnyAsync(d => d.DepartmentId == departmentId && !d.IsDeleted);
        }

        public async Task<bool> IsActiveAgentInDepartmentAsync(string employeeId, Guid departmentId)
        {
            // "active licensed employee" = not deleted Employee row whose linked
            // ApplicationUser has login access and Status == true.
            return await _context.Set<Employee>()
                .Where(e => e.Id == employeeId && !e.IsDeleted && e.DepartmentId == departmentId)
                .Join(_context.Users,
                    e => e.Id,
                    u => u.Id,
                    (e, u) => u)
                .AnyAsync(u => u.Status && u.HasLoginAccess);
        }
    }
}
