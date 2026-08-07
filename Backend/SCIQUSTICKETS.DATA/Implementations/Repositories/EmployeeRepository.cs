using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
	public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
	{
		public EmployeeRepository(AppDbContext context) : base(context) { }

		public async Task<(List<Employee> Items, int TotalCount)> GetAllPagedAsync(
			Guid? departmentId, Guid? gradeId, string? reportsTo,
			bool? isDeleted, string? search,
			string sortBy, bool sortDescending, int page, int pageSize)
		{
			var query = _context.Employees
				.Include(e => e.Department)
				.Include(e => e.Grade)
				.AsQueryable();

			if (isDeleted.HasValue)
				query = query.Where(e => e.IsDeleted == isDeleted.Value);

			if (departmentId.HasValue)
				query = query.Where(e => e.DepartmentId == departmentId.Value);

			if (gradeId.HasValue)
				query = query.Where(e => e.GradeId == gradeId.Value);

			if (!string.IsNullOrWhiteSpace(reportsTo))
				query = query.Where(e => e.ReportsTo == reportsTo);

			if (!string.IsNullOrWhiteSpace(search))
				query = query.Where(e =>
					e.Name.Contains(search) ||
					e.Email.Contains(search) ||
					(e.EmployeeId != null && e.EmployeeId.Contains(search)));

			var totalCount = await query.CountAsync();

			query = sortBy?.ToLower() switch
			{
				"createddate" => sortDescending
					? query.OrderByDescending(e => e.CreatedDate)
					: query.OrderBy(e => e.CreatedDate),
				"designation" => sortDescending
					? query.OrderByDescending(e => e.Designation)
					: query.OrderBy(e => e.Designation),
				_ => sortDescending
					? query.OrderByDescending(e => e.Name)
					: query.OrderBy(e => e.Name),
			};

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		public async Task<List<Employee>> GetDirectReportsAsync(string employeeId)
		{
			return await _context.Employees
				.Where(e => e.ReportsTo == employeeId && !e.IsDeleted)
				.ToListAsync();
		}

		public async Task<bool> SoftDeleteAsync(string id)
		{
			var employee = await _context.Employees.FindAsync(id);
			if (employee == null) return false;

			employee.IsDeleted = true;
			employee.LastUpdatedDate = DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> IsCircularReportingAsync(string employeeId, string proposedManagerId)
		{
			var currentId = proposedManagerId;
			var visited = new HashSet<string>();

			while (!string.IsNullOrEmpty(currentId))
			{
				if (currentId == employeeId)
					return true;

				if (!visited.Add(currentId))
					break;

				var manager = await _context.Employees
					.Where(e => e.Id == currentId)
					.Select(e => new { e.ReportsTo })
					.FirstOrDefaultAsync();

				currentId = manager?.ReportsTo;
			}

			return false;
		}
	}
}