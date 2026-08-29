using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.HolidayRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.HolidayResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IHolidayService
	{
		// ---- Admin: calendar CRUD ----
		Task<IEnumerable<HolidayResponse>> GetAllAsync();
		Task<HolidayResponse?> GetByIdAsync(Guid id);
		Task<HolidayResponse> CreateAsync(CreateHolidayRequest request);
		Task<HolidayResponse> UpdateAsync(Guid id, UpdateHolidayRequest request);
		Task<bool> SoftDeleteAsync(Guid id);

		// ---- Employee: confirmations ----
		Task<IEnumerable<HolidayConfirmationResponse>> GetConfirmationsForEmployeeAsync(string employeeId);
		Task<HolidayConfirmationResponse> ConfirmAsync(string employeeId, Guid holidayId, ConfirmHolidayRequest request);
	}
}