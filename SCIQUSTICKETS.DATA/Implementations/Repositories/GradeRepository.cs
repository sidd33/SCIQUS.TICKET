using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
	public class GradeRepository : GenericRepository<Grade>, IGradeRepository
	{
		public GradeRepository(AppDbContext context) : base(context) { }

		public async Task<List<Grade>> GetAllAsync(bool? isDeleted)
		{
			var query = _context.Grades.AsQueryable();

			if (isDeleted.HasValue)
				query = query.Where(g => g.IsDeleted == isDeleted.Value);

			return await query.OrderBy(g => g.GradeLevel).ToListAsync();
		}

		public async Task<bool> SoftDeleteAsync(Guid id)
		{
			var grade = await _context.Grades.FindAsync(id);
			if (grade == null) return false;

			grade.IsDeleted = true;
			grade.LastUpdatedDate = DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return true;
		}
	}
}