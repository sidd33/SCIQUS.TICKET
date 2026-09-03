using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class DepartmentService : IDepartmentService
	{
		private readonly IDepartmentRepository _departmentRepository;
		private readonly IEmployeeRepository _employeeRepository;

		public DepartmentService(IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository)
		{
			_departmentRepository = departmentRepository;
			_employeeRepository = employeeRepository;
		}

		public async Task<DepartmentResponse?> GetByIdAsync(Guid id)
		{
			var department = await _departmentRepository.GetByIdAsync(id);
			if (department == null) return null;

			var count = await _departmentRepository.GetEmployeeCountAsync(id);
			return MapToResponse(department, count);
		}

		public async Task<PagedResponse<DepartmentListResponse>> GetAllAsync(DepartmentQueryParams queryParams)
		{
			var (items, totalCount) = await _departmentRepository.GetAllPagedAsync(
				queryParams.IsDeleted, queryParams.Search,
				queryParams.SortBy ?? "Name", queryParams.SortDescending,
				queryParams.Page, queryParams.PageSize);

			var responses = new List<DepartmentListResponse>();
			foreach (var d in items)
			{
				var count = await _departmentRepository.GetEmployeeCountAsync(d.DepartmentId);
				responses.Add(new DepartmentListResponse
				{
					DepartmentId = d.DepartmentId,
					Name = d.Name,
					EmployeeCount = count,
					TicketAutoAssignMethod = d.TicketAutoAssignMethod,
					W_Load = d.W_Load,
					W_Severity = d.W_Severity,
					W_Recency = d.W_Recency
				});
			}

			return new PagedResponse<DepartmentListResponse>
			{
				Items = responses,
				TotalCount = totalCount,
				Page = queryParams.Page,
				PageSize = queryParams.PageSize
			};
		}

		public async Task<List<EmployeeListResponse>> GetEmployeesInDepartmentAsync(Guid departmentId)
		{
			var (items, _) = await _employeeRepository.GetAllPagedAsync(
				departmentId, null, null, false, null, "Name", false, 1, int.MaxValue);

			return items.Select(e => new EmployeeListResponse
			{
				Id = e.Id,
				Name = e.Name,
				Designation = e.Designation,
				DepartmentName = e.Department?.Name,
				ProfileImageUrl = e.ProfileImageUrl
			}).ToList();
		}

		public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request)
		{
			var department = new Department
			{
				DepartmentId = Guid.NewGuid(),
				Name = request.Name,
				DepartmentHeadId = request.DepartmentHeadId,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _departmentRepository.AddAsync(department);
			await _departmentRepository.SaveChangesAsync();
			return MapToResponse(department, 0);
		}

		public async Task<DepartmentResponse> UpdateAsync(Guid id, UpdateDepartmentRequest request)
		{
			var department = await _departmentRepository.GetByIdAsync(id)
				?? throw new KeyNotFoundException($"Department {id} not found.");

			if (request.Name != null)
				department.Name = request.Name;

			if (request.TicketAutoAssignMethod != null)
				department.TicketAutoAssignMethod = request.TicketAutoAssignMethod;

			if (request.W_Load.HasValue)
				department.W_Load = request.W_Load.Value;

			if (request.W_Severity.HasValue)
				department.W_Severity = request.W_Severity.Value;

			if (request.W_Recency.HasValue)
				department.W_Recency = request.W_Recency.Value;

			department.LastModifiedDate = DateTime.UtcNow;

			_departmentRepository.Update(department);
			await _departmentRepository.SaveChangesAsync();

			var count = await _departmentRepository.GetEmployeeCountAsync(id);
			return MapToResponse(department, count);
		}

		public async Task<bool> SetHeadAsync(Guid departmentId, SetDepartmentHeadRequest request)
		{
			return await _departmentRepository.SetHeadAsync(departmentId, request.DepartmentHeadId);
		}

		public async Task<bool> SoftDeleteAsync(Guid id)
		{
			return await _departmentRepository.SoftDeleteAsync(id);
		}

		private static DepartmentResponse MapToResponse(Department d, int employeeCount) => new()
		{
			DepartmentId = d.DepartmentId,
			Name = d.Name,
			DepartmentHeadId = Guid.TryParse(d.DepartmentHeadId, out var headId)
		? headId
		: null,
			DepartmentHeadName = d.DepartmentHead?.Name,
			EmployeeCount = employeeCount,
			CreatedDate = d.CreatedDate,
			LastModifiedDate = d.LastModifiedDate,

			TicketAutoAssignMethod = d.TicketAutoAssignMethod,
			W_Load = d.W_Load,
			W_Severity = d.W_Severity,
			W_Recency = d.W_Recency
		};
	}
}