using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
	public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
	{
		public DepartmentRepository(AppDbContext context) : base(context) { }

		public async Task<(List<Department> Items, int TotalCount)> GetAllPagedAsync(
			bool? isDeleted, string? search,
			string sortBy, bool sortDescending, int page, int pageSize)
		{
			var query = _context.Departments
				.Include(d => d.DepartmentHead)
				.AsQueryable();

			if (isDeleted.HasValue)
				query = query.Where(d => d.IsDeleted == isDeleted.Value);

			if (!string.IsNullOrWhiteSpace(search))
				query = query.Where(d => d.Name.Contains(search));

			var totalCount = await query.CountAsync();

			query = sortBy?.ToLower() switch
			{
				"createddate" => sortDescending
					? query.OrderByDescending(d => d.CreatedDate)
					: query.OrderBy(d => d.CreatedDate),
				_ => sortDescending
					? query.OrderByDescending(d => d.Name)
					: query.OrderBy(d => d.Name),
			};

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		public async Task<bool> SetHeadAsync(Guid departmentId, string employeeId)
		{
			var department = await _context.Departments.FindAsync(departmentId);
			if (department == null) return false;

			department.DepartmentHeadId = employeeId;
			department.LastModifiedDate = DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> SoftDeleteAsync(Guid id)
		{
			var department = await _context.Departments.FindAsync(id);
			if (department == null) return false;

			department.IsDeleted = true;
			department.LastModifiedDate = DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<int> GetEmployeeCountAsync(Guid departmentId)
		{
			return await _context.Employees
				.CountAsync(e => e.DepartmentId == departmentId && !e.IsDeleted);
		}
	}
}