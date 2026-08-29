using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;
using Microsoft.AspNetCore.Identity;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public EmployeeService(
	IEmployeeRepository employeeRepository,
	AppDbContext context,
	UserManager<ApplicationUser> userManager)
		{
			_employeeRepository = employeeRepository;
			_context = context;
			_userManager = userManager;
		}

		// ============================================================
		// EXISTING EMPLOYEE OPERATIONS
		// ============================================================

		public async Task<EmployeeResponse?> GetByIdAsync(string id)
		{
			var employee = await _employeeRepository.GetByIdAsync(id);

			return employee == null
				? null
				: MapToResponse(employee);
		}

		public async Task<PagedResponse<EmployeeListResponse>> GetAllAsync(
			EmployeeQueryParams queryParams)
		{
			var (items, totalCount) =
				await _employeeRepository.GetAllPagedAsync(
					queryParams.DepartmentId,
					queryParams.GradeId,
					queryParams.ReportsTo,
					queryParams.IsDeleted,
					queryParams.Search,
					queryParams.SortBy ?? "Name",
					queryParams.SortDescending,
					queryParams.Page,
					queryParams.PageSize);

			return new PagedResponse<EmployeeListResponse>
			{
				Items = items.Select(MapToListResponse).ToList(),
				TotalCount = totalCount,
				Page = queryParams.Page,
				PageSize = queryParams.PageSize
			};
		}

		public async Task<List<EmployeeListResponse>> GetDirectReportsAsync(
			string employeeId)
		{
			var reports =
				await _employeeRepository.GetDirectReportsAsync(employeeId);

			return reports.Select(MapToListResponse).ToList();
		}

		public async Task<EmployeeResponse> CreateAsync(
	CreateEmployeeRequest request)
		{
			// Check whether an account with this email already exists
			var existingUser = await _userManager.FindByEmailAsync(request.Email);

			if (existingUser != null)
			{
				throw new InvalidOperationException(
					"A user with this email address already exists.");
			}

			// Create the login account
			var applicationUser = new ApplicationUser
			{
				UserName = request.Email,
				Email = request.Email,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow,
				Status = true,
				HasLoginAccess = true
			};

			var userResult = await _userManager.CreateAsync(
				applicationUser,
				request.Password);

			if (!userResult.Succeeded)
			{
				var errors = string.Join(
					"; ",
					userResult.Errors.Select(e => e.Description));

				throw new InvalidOperationException(
					$"Unable to create login account: {errors}");
			}

			try
			{
				// Create the employee profile using the newly-created
				// ApplicationUser ID.
				var employee = new Employee
				{
					Id = applicationUser.Id,
					Name = request.Name,
					RegisteredMobileNumber = request.RegisteredMobileNumber,
					SecondMobileNumber = request.SecondMobileNumber,
					Email = request.Email,
					EmployeeId = request.EmployeeId,
					Designation = request.Designation,
					ReportsTo = request.ReportsTo,
					DepartmentId = request.DepartmentId,
					GradeId = request.GradeId,
					ProfileImageUrl = request.ProfileImageUrl,
					AutoGenrateId = Guid.NewGuid().ToString(),
					CreatedDate = DateTime.UtcNow,
					LastUpdatedDate = DateTime.UtcNow
				};

				await _employeeRepository.AddAsync(employee);
				await _employeeRepository.SaveChangesAsync();

				return MapToResponse(employee);
			}
			catch
			{
				// If Employee creation fails, remove the ApplicationUser
				// so we don't leave an orphaned login account.
				await _userManager.DeleteAsync(applicationUser);

				throw;
			}
		}

		public async Task<EmployeeResponse> UpdateAsync(
			string id,
			UpdateEmployeeRequest request)
		{
			var employee =
				await _employeeRepository.GetByIdAsync(id)
				?? throw new KeyNotFoundException(
					$"Employee {id} not found.");

			if (!string.IsNullOrEmpty(request.ReportsTo) &&
				request.ReportsTo != employee.ReportsTo)
			{
				var wouldCycle =
					await _employeeRepository.IsCircularReportingAsync(
						id,
						request.ReportsTo);

				if (wouldCycle)
				{
					throw new InvalidOperationException(
						"This assignment would create a circular reporting chain.");
				}

				employee.ReportsTo = request.ReportsTo;
			}

			if (request.Name != null)
				employee.Name = request.Name;

			if (request.RegisteredMobileNumber != null)
				employee.RegisteredMobileNumber =
					request.RegisteredMobileNumber;

			if (request.SecondMobileNumber != null)
				employee.SecondMobileNumber =
					request.SecondMobileNumber;

			if (request.Designation != null)
				employee.Designation = request.Designation;

			if (request.DepartmentId.HasValue)
				employee.DepartmentId = request.DepartmentId.Value;

			if (request.GradeId.HasValue)
				employee.GradeId = request.GradeId;

			if (request.ProfileImageUrl != null)
				employee.ProfileImageUrl =
					request.ProfileImageUrl;

			employee.LastUpdatedDate = DateTime.UtcNow;

			_employeeRepository.Update(employee);

			await _employeeRepository.SaveChangesAsync();

			return MapToResponse(employee);
		}

		public async Task<bool> SoftDeleteAsync(string id)
		{
			return await _employeeRepository.SoftDeleteAsync(id);
		}


		// ============================================================
		// WORKING HOURS
		// ============================================================

		public async Task<List<EmployeeWorkingHourResponse>>
			GetWorkingHoursAsync(string employeeId)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			var workingHours =
				await _context.EmployeeWorkingHours
					.AsNoTracking()
					.Where(w => w.EmployeeId == employeeId)
					.OrderBy(w => w.DayOfWeek)
					.ToListAsync();

			return workingHours
				.Select(MapWorkingHourToResponse)
				.ToList();
		}


		public async Task<EmployeeWorkingHourResponse>
			AddWorkingHourAsync(
				string employeeId,
				CreateEmployeeWorkingHourRequest request)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			ValidateWorkingHour(request.StartTime, request.EndTime);

			// Prevent duplicate working-hour entries
			// for the same employee and day.
			var existing =
				await _context.EmployeeWorkingHours
					.AnyAsync(w =>
						w.EmployeeId == employeeId &&
						w.DayOfWeek == request.DayOfWeek);

			if (existing)
			{
				throw new InvalidOperationException(
					$"Working hours already exist for {request.DayOfWeek}.");
			}

			var workingHour = new EmployeeWorkingHour
			{
				Id = Guid.NewGuid(),
				EmployeeId = employeeId,
				DayOfWeek = request.DayOfWeek,
				StartTime = request.StartTime,
				EndTime = request.EndTime,
				IsWorkingDay = request.IsWorkingDay
			};

			_context.EmployeeWorkingHours.Add(workingHour);

			await _context.SaveChangesAsync();

			return MapWorkingHourToResponse(workingHour);
		}


		public async Task<EmployeeWorkingHourResponse>
			UpdateWorkingHourAsync(
				string employeeId,
				Guid workingHourId,
				UpdateEmployeeWorkingHourRequest request)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			ValidateWorkingHour(request.StartTime, request.EndTime);

			var workingHour =
				await _context.EmployeeWorkingHours
					.FirstOrDefaultAsync(w =>
						w.Id == workingHourId &&
						w.EmployeeId == employeeId);

			if (workingHour == null)
			{
				throw new KeyNotFoundException(
					$"Working hour {workingHourId} not found.");
			}

			var duplicate =
				await _context.EmployeeWorkingHours
					.AnyAsync(w =>
						w.Id != workingHourId &&
						w.EmployeeId == employeeId &&
						w.DayOfWeek == request.DayOfWeek);

			if (duplicate)
			{
				throw new InvalidOperationException(
					$"Working hours already exist for {request.DayOfWeek}.");
			}

			workingHour.DayOfWeek = request.DayOfWeek;
			workingHour.StartTime = request.StartTime;
			workingHour.EndTime = request.EndTime;
			workingHour.IsWorkingDay = request.IsWorkingDay;

			await _context.SaveChangesAsync();

			return MapWorkingHourToResponse(workingHour);
		}


		public async Task<bool> DeleteWorkingHourAsync(
			string employeeId,
			Guid workingHourId)
		{
			var workingHour =
				await _context.EmployeeWorkingHours
					.FirstOrDefaultAsync(w =>
						w.Id == workingHourId &&
						w.EmployeeId == employeeId);

			if (workingHour == null)
				return false;

			_context.EmployeeWorkingHours.Remove(workingHour);

			await _context.SaveChangesAsync();

			return true;
		}


		// ============================================================
		// LEAVE
		// ============================================================

		public async Task<List<EmployeeLeaveResponse>>
			GetLeavesAsync(string employeeId)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			var leaves =
				await _context.EmployeeLeaves
					.AsNoTracking()
					.Where(l =>
						l.EmployeeId == employeeId &&
						!l.IsDeleted)
					.OrderByDescending(l => l.StartDate)
					.ToListAsync();

			return leaves
				.Select(MapLeaveToResponse)
				.ToList();
		}


		public async Task<EmployeeLeaveResponse>
			AddLeaveAsync(
				string employeeId,
				CreateEmployeeLeaveRequest request)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			ValidateLeaveDates(
				request.StartDate,
				request.EndDate);

			// Check for overlapping active leave.
			var hasOverlap =
				await _context.EmployeeLeaves
					.AnyAsync(l =>
						l.EmployeeId == employeeId &&
						!l.IsDeleted &&
						l.StartDate <= request.EndDate &&
						l.EndDate >= request.StartDate);

			if (hasOverlap)
			{
				throw new InvalidOperationException(
					"The employee already has leave during this period.");
			}

			var leave = new EmployeeLeave
			{
				Id = Guid.NewGuid(),
				EmployeeId = employeeId,
				StartDate = request.StartDate,
				EndDate = request.EndDate,
				LeaveType = request.LeaveType,

				// Employee requests leave.
				// Approval happens separately.
				Status = "Pending",

				IsDeleted = false,
				CreatedDate = TimeHelper.GetIndianTime()
			};

			_context.EmployeeLeaves.Add(leave);

			await _context.SaveChangesAsync();

			return MapLeaveToResponse(leave);
		}


		public async Task<EmployeeLeaveResponse>
	UpdateLeaveAsync(
		string employeeId,
		Guid leaveId,
		UpdateEmployeeLeaveRequest request)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			ValidateLeaveDates(
				request.StartDate,
				request.EndDate);

			var leave =
				await _context.EmployeeLeaves
					.FirstOrDefaultAsync(l =>
						l.Id == leaveId &&
						l.EmployeeId == employeeId &&
						!l.IsDeleted);

			if (leave == null)
			{
				throw new KeyNotFoundException(
					$"Leave {leaveId} not found.");
			}

			var hasOverlap =
				await _context.EmployeeLeaves
					.AnyAsync(l =>
						l.Id != leaveId &&
						l.EmployeeId == employeeId &&
						!l.IsDeleted &&
						l.StartDate <= request.EndDate &&
						l.EndDate >= request.StartDate);

			if (hasOverlap)
			{
				throw new InvalidOperationException(
					"The employee already has another leave during this period.");
			}

			leave.StartDate = request.StartDate;
			leave.EndDate = request.EndDate;
			leave.LeaveType = request.LeaveType;

			if (!string.IsNullOrWhiteSpace(request.Status))
			{
				var allowedStatuses = new[]
				{
			"Pending",
			"Approved",
			"Rejected",
			"Cancelled"
		};

				if (!allowedStatuses.Contains(request.Status))
				{
					throw new ArgumentException(
						$"Invalid leave status: {request.Status}");
				}

				leave.Status = request.Status;
			}

			await _context.SaveChangesAsync();

			return MapLeaveToResponse(leave);
		}

		


		public async Task<bool> DeleteLeaveAsync(
			string employeeId,
			Guid leaveId)
		{
			var leave =
				await _context.EmployeeLeaves
					.FirstOrDefaultAsync(l =>
						l.Id == leaveId &&
						l.EmployeeId == employeeId &&
						!l.IsDeleted);

			if (leave == null)
				return false;

			// Don't delete approved leave directly.
			if (leave.Status == "Approved")
			{
				throw new InvalidOperationException(
					"Approved leave cannot be deleted.");
			}

			// Soft delete.
			leave.IsDeleted = true;

			await _context.SaveChangesAsync();

			return true;
		}

		// ============================================================
		// EMAIL NOTIFICATION PREFERENCES
		// ============================================================

		public async Task<EmployeeEmailNotificationPreference?>
			GetEmailNotificationPreferenceAsync(string employeeId)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			return await _context.EmployeeEmailNotificationPreferences
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.EmployeeId == employeeId);
		}


		public async Task<EmployeeEmailNotificationPreference>
			SaveEmailNotificationPreferenceAsync(
				string employeeId,
				EmployeeEmailNotificationPreference preference)
		{
			await EnsureEmployeeExistsAsync(employeeId);

			var existingPreference =
				await _context.EmployeeEmailNotificationPreferences
					.FirstOrDefaultAsync(p => p.EmployeeId == employeeId);

			var now = TimeHelper.GetIndianTime();

			if (existingPreference == null)
			{
				preference.EmployeeEmailNotificationPreferenceId = Guid.NewGuid();
				preference.EmployeeId = employeeId;
				preference.CreatedDate = now;
				preference.LastUpdatedDate = now;

				_context.EmployeeEmailNotificationPreferences.Add(preference);
			}
			else
			{
				existingPreference.ReceiveAll = preference.ReceiveAll;
				existingPreference.Assignment = preference.Assignment;
				existingPreference.Acceptance = preference.Acceptance;
				existingPreference.Rejection = preference.Rejection;
				existingPreference.Expiry = preference.Expiry;
				existingPreference.Reassignment = preference.Reassignment;
				existingPreference.StatusChange = preference.StatusChange;
				existingPreference.Closure = preference.Closure;
				existingPreference.Reopen = preference.Reopen;
				existingPreference.LastUpdatedDate = now;

				preference = existingPreference;
			}

			await _context.SaveChangesAsync();

			return preference;
		}


		// ============================================================
		// VALIDATION / HELPERS
		// ============================================================

		private async Task EnsureEmployeeExistsAsync(
			string employeeId)
		{
			var exists =
				await _context.Employees
					.AsNoTracking()
					.AnyAsync(e =>
						e.Id == employeeId &&
						!e.IsDeleted);

			if (!exists)
			{
				throw new KeyNotFoundException(
					$"Employee {employeeId} not found.");
			}
		}


		private static void ValidateWorkingHour(
			TimeSpan startTime,
			TimeSpan endTime)
		{
			if (startTime == endTime)
			{
				throw new ArgumentException(
					"Start time and end time cannot be the same.");
			}
		}


		private static void ValidateLeaveDates(
			DateTime startDate,
			DateTime endDate)
		{
			if (endDate < startDate)
			{
				throw new ArgumentException(
					"Leave end date cannot be before start date.");
			}
		}


		// ============================================================
		// MAPPERS
		// ============================================================

		private static EmployeeWorkingHourResponse
			MapWorkingHourToResponse(
				EmployeeWorkingHour workingHour)
		{
			return new EmployeeWorkingHourResponse
			{
				Id = workingHour.Id,
				EmployeeId = workingHour.EmployeeId,
				DayOfWeek = workingHour.DayOfWeek,
				StartTime = workingHour.StartTime,
				EndTime = workingHour.EndTime,
				IsWorkingDay = workingHour.IsWorkingDay
			};
		}


		private static EmployeeLeaveResponse
			MapLeaveToResponse(
				EmployeeLeave leave)
		{
			return new EmployeeLeaveResponse
			{
				Id = leave.Id,
				EmployeeId = leave.EmployeeId,
				StartDate = leave.StartDate,
				EndDate = leave.EndDate,
				LeaveType = leave.LeaveType,
				Status = leave.Status,
				IsDeleted = leave.IsDeleted,
				CreatedDate = leave.CreatedDate
			};
		}


		// ============================================================
		// EXISTING MAPPERS
		// ============================================================

		private static EmployeeResponse
			MapToResponse(Employee e)
		{
			return new EmployeeResponse
			{
				Id = e.Id,
				Name = e.Name,
				RegisteredMobileNumber =
					e.RegisteredMobileNumber,
				SecondMobileNumber =
					e.SecondMobileNumber,
				Email = e.Email,
				EmployeeId = e.EmployeeId,
				Designation = e.Designation,
				ReportsTo = e.ReportsTo,
				ReportsToName = e.ReportsToUser?.Name,
				DepartmentId = e.DepartmentId,
				DepartmentName = e.Department?.Name,
				GradeId = e.GradeId,
				GradeLevel = e.Grade?.GradeLevel,
				ProfileImageUrl = e.ProfileImageUrl,
				CreatedDate = e.CreatedDate,
				LastUpdatedDate = e.LastUpdatedDate
			};
		}


		private static EmployeeListResponse
	MapToListResponse(Employee e)
		{
			return new EmployeeListResponse
			{
				Id = e.Id,
				Name = e.Name,
				Email = e.Email,
				Designation = e.Designation,
				DepartmentName = e.Department?.Name,
				ProfileImageUrl = e.ProfileImageUrl
			};
		}
	}
}
