using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
	public interface IEmployeeRepository : IGenericRepository<Employee>
	{
		Task<(List<Employee> Items, int TotalCount)> GetAllPagedAsync(
			Guid? departmentId, Guid? gradeId, string? reportsTo,
			bool? isDeleted, string? search,
			string sortBy, bool sortDescending, int page, int pageSize);
		Task<List<Employee>> GetDirectReportsAsync(string employeeId);
		Task<bool> SoftDeleteAsync(string id);
		Task<bool> IsCircularReportingAsync(string employeeId, string proposedManagerId);
	}
}