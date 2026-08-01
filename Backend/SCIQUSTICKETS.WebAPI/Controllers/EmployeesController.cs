using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class EmployeesController : ControllerBase
	{
		private readonly IEmployeeService _employeeService;

		public EmployeesController(IEmployeeService employeeService)
		{
			_employeeService = employeeService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] EmployeeQueryParams queryParams)
		{
			var result = await _employeeService.GetAllAsync(queryParams);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(string id)
		{
			var employee = await _employeeService.GetByIdAsync(id);
			if (employee == null) return NotFound();
			return Ok(employee);
		}

		[HttpGet("{id}/reports")]
		public async Task<IActionResult> GetDirectReports(string id)
		{
			var reports = await _employeeService.GetDirectReportsAsync(id);
			return Ok(reports);
		}

		// applicationUserId identifies which already-registered ApplicationUser this
		// Employee record belongs to — TODO: confirm exact flow with Auth teammate
		// (e.g. does registration auto-create the Employee, or is this a separate admin step?)
		[HttpPost("{applicationUserId}")]
		public async Task<IActionResult> Create(string applicationUserId, [FromBody] CreateEmployeeRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var created = await _employeeService.CreateAsync(applicationUserId, request);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(string id, [FromBody] UpdateEmployeeRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var updated = await _employeeService.UpdateAsync(id, request);
			return Ok(updated);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			var success = await _employeeService.SoftDeleteAsync(id);
			if (!success) return NotFound();
			return NoContent();
		}
	}
}