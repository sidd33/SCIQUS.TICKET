using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
	public interface IDepartmentRepository : IGenericRepository<Department>
	{
		Task<(List<Department> Items, int TotalCount)> GetAllPagedAsync(
			bool? isDeleted, string? search,
			string sortBy, bool sortDescending, int page, int pageSize);
		Task<bool> SetHeadAsync(Guid departmentId, string employeeId);
		Task<bool> SoftDeleteAsync(Guid id);
		Task<int> GetEmployeeCountAsync(Guid departmentId);
	}
}