using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class DepartmentsController : ControllerBase
	{
		private readonly IDepartmentService _departmentService;

		public DepartmentsController(IDepartmentService departmentService)
		{
			_departmentService = departmentService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] DepartmentQueryParams queryParams)
		{
			var result = await _departmentService.GetAllAsync(queryParams);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var department = await _departmentService.GetByIdAsync(id);
			if (department == null) return NotFound();
			return Ok(department);
		}

		[HttpGet("{id}/employees")]
		public async Task<IActionResult> GetEmployees(Guid id)
		{
			var employees = await _departmentService.GetEmployeesInDepartmentAsync(id);
			return Ok(employees);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var created = await _departmentService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var updated = await _departmentService.UpdateAsync(id, request);
			return Ok(updated);
		}

		[HttpPatch("{id}/head")]
		public async Task<IActionResult> SetHead(Guid id, [FromBody] SetDepartmentHeadRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var success = await _departmentService.SetHeadAsync(id, request);
			if (!success) return NotFound();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var success = await _departmentService.SoftDeleteAsync(id);
			if (!success) return NotFound();
			return NoContent();
		}
	}
}