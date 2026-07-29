using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IEmployeeService
	{
		Task<EmployeeResponse?> GetByIdAsync(string id);
		Task<PagedResponse<EmployeeListResponse>> GetAllAsync(EmployeeQueryParams queryParams);
		Task<List<EmployeeListResponse>> GetDirectReportsAsync(string employeeId);
		Task<EmployeeResponse> CreateAsync(string applicationUserId, CreateEmployeeRequest request);
		Task<EmployeeResponse> UpdateAsync(string id, UpdateEmployeeRequest request);
		Task<bool> SoftDeleteAsync(string id);
	}
}