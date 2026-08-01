using System;
using System.Collections.Generic;
using System.Text;

using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IDepartmentService
	{
		Task<DepartmentResponse?> GetByIdAsync(Guid id);
		Task<PagedResponse<DepartmentListResponse>> GetAllAsync(DepartmentQueryParams queryParams);
		Task<List<EmployeeListResponse>> GetEmployeesInDepartmentAsync(Guid departmentId);
		Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request);
		Task<DepartmentResponse> UpdateAsync(Guid id, UpdateDepartmentRequest request);
		Task<bool> SetHeadAsync(Guid departmentId, SetDepartmentHeadRequest request);
		Task<bool> SoftDeleteAsync(Guid id);
	}
}