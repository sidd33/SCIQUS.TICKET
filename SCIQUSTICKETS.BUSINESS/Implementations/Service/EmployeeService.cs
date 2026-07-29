using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IEmployeeRepository _employeeRepository;

		public EmployeeService(IEmployeeRepository employeeRepository)
		{
			_employeeRepository = employeeRepository;
		}

		public async Task<EmployeeResponse?> GetByIdAsync(string id)
		{
			var employee = await _employeeRepository.GetByIdAsync(id);
			return employee == null ? null : MapToResponse(employee);
		}

		public async Task<PagedResponse<EmployeeListResponse>> GetAllAsync(EmployeeQueryParams queryParams)
		{
			var (items, totalCount) = await _employeeRepository.GetAllPagedAsync(
				queryParams.DepartmentId, queryParams.GradeId, queryParams.ReportsTo,
				queryParams.IsDeleted, queryParams.Search,
				queryParams.SortBy ?? "Name", queryParams.SortDescending,
				queryParams.Page, queryParams.PageSize);

			return new PagedResponse<EmployeeListResponse>
			{
				Items = items.Select(MapToListResponse).ToList(),
				TotalCount = totalCount,
				Page = queryParams.Page,
				PageSize = queryParams.PageSize
			};
		}

		public async Task<List<EmployeeListResponse>> GetDirectReportsAsync(string employeeId)
		{
			var reports = await _employeeRepository.GetDirectReportsAsync(employeeId);
			return reports.Select(MapToListResponse).ToList();
		}

		// applicationUserId comes from the already-registered ApplicationUser (via AuthController.Register)
		// Employee.Id MUST match ApplicationUser.Id — confirm this flow with the Auth teammate
		public async Task<EmployeeResponse> CreateAsync(string applicationUserId, CreateEmployeeRequest request)
		{
			var employee = new Employee
			{
				Id = applicationUserId,
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
				AutoGenrateId = Guid.NewGuid().ToString(), // TODO: confirm actual generation rule with team
				CreatedDate = DateTime.UtcNow,
				LastUpdatedDate = DateTime.UtcNow
			};

			await _employeeRepository.AddAsync(employee);
			await _employeeRepository.SaveChangesAsync();
			return MapToResponse(employee);
		}

		public async Task<EmployeeResponse> UpdateAsync(string id, UpdateEmployeeRequest request)
		{
			var employee = await _employeeRepository.GetByIdAsync(id)
				?? throw new KeyNotFoundException($"Employee {id} not found.");

			if (!string.IsNullOrEmpty(request.ReportsTo) && request.ReportsTo != employee.ReportsTo)
			{
				var wouldCycle = await _employeeRepository.IsCircularReportingAsync(id, request.ReportsTo);
				if (wouldCycle)
					throw new InvalidOperationException("This assignment would create a circular reporting chain.");
				employee.ReportsTo = request.ReportsTo;
			}

			if (request.Name != null) employee.Name = request.Name;
			if (request.RegisteredMobileNumber != null) employee.RegisteredMobileNumber = request.RegisteredMobileNumber;
			if (request.SecondMobileNumber != null) employee.SecondMobileNumber = request.SecondMobileNumber;
			if (request.Designation != null) employee.Designation = request.Designation;
			if (request.DepartmentId.HasValue) employee.DepartmentId = request.DepartmentId.Value;
			if (request.GradeId.HasValue) employee.GradeId = request.GradeId;
			if (request.ProfileImageUrl != null) employee.ProfileImageUrl = request.ProfileImageUrl;

			employee.LastUpdatedDate = DateTime.UtcNow;
			_employeeRepository.Update(employee);
			await _employeeRepository.SaveChangesAsync();
			return MapToResponse(employee);
		}

		public async Task<bool> SoftDeleteAsync(string id)
		{
			return await _employeeRepository.SoftDeleteAsync(id);
		}

		private static EmployeeResponse MapToResponse(Employee e) => new()
		{
			Id = e.Id,
			Name = e.Name,
			RegisteredMobileNumber = e.RegisteredMobileNumber,
			SecondMobileNumber = e.SecondMobileNumber,
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

		private static EmployeeListResponse MapToListResponse(Employee e) => new()
		{
			Id = e.Id,
			Name = e.Name,
			Designation = e.Designation,
			DepartmentName = e.Department?.Name,
			ProfileImageUrl = e.ProfileImageUrl
		};
	}
}