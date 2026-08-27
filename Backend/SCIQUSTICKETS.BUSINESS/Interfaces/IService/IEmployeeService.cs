using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IEmployeeService
	{
		// Existing employee operations
		Task<EmployeeResponse?> GetByIdAsync(string id);

		Task<PagedResponse<EmployeeListResponse>> GetAllAsync(
			EmployeeQueryParams queryParams);

		Task<List<EmployeeListResponse>> GetDirectReportsAsync(
			string employeeId);

		Task<EmployeeResponse> CreateAsync(
	CreateEmployeeRequest request);

		Task<EmployeeResponse> UpdateAsync(
			string id,
			UpdateEmployeeRequest request);

		Task<bool> SoftDeleteAsync(string id);


		// ============================================================
		// WORKING HOURS
		// ============================================================

		Task<List<EmployeeWorkingHourResponse>> GetWorkingHoursAsync(
			string employeeId);

		Task<EmployeeWorkingHourResponse> AddWorkingHourAsync(
			string employeeId,
			CreateEmployeeWorkingHourRequest request);

		Task<EmployeeWorkingHourResponse> UpdateWorkingHourAsync(
			string employeeId,
			Guid workingHourId,
			UpdateEmployeeWorkingHourRequest request);

		Task<bool> DeleteWorkingHourAsync(
			string employeeId,
			Guid workingHourId);


		// ============================================================
		// LEAVE
		// ============================================================

		Task<List<EmployeeLeaveResponse>> GetLeavesAsync(
			string employeeId);

		Task<EmployeeLeaveResponse> AddLeaveAsync(
			string employeeId,
			CreateEmployeeLeaveRequest request);

		Task<EmployeeLeaveResponse> UpdateLeaveAsync(
			string employeeId,
			Guid leaveId,
			UpdateEmployeeLeaveRequest request);

		Task<bool> DeleteLeaveAsync(
			string employeeId,
			Guid leaveId);

		Task<EmployeeEmailNotificationPreference?> GetEmailNotificationPreferenceAsync(
	string employeeId);

		Task<EmployeeEmailNotificationPreference> SaveEmailNotificationPreferenceAsync(
			string employeeId,
			EmployeeEmailNotificationPreference preference);
	}
}
