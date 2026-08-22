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
		[HttpPost]
		public async Task<IActionResult> Create(
	[FromBody] CreateEmployeeRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var created = await _employeeService.CreateAsync(request);

				return CreatedAtAction(
					nameof(GetById),
					new { id = created.Id },
					created);
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new
				{
					message = ex.Message
				});
			}
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

		// ============================================================
		// WORKING HOURS
		// ============================================================

		[HttpGet("{id}/working-hours")]
		public async Task<IActionResult> GetWorkingHours(string id)
		{
			var workingHours =
				await _employeeService.GetWorkingHoursAsync(id);

			return Ok(workingHours);
		}


		[HttpPost("{id}/working-hours")]
		public async Task<IActionResult> AddWorkingHour(
			string id,
			[FromBody] CreateEmployeeWorkingHourRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var created =
					await _employeeService.AddWorkingHourAsync(
						id,
						request);

				return Ok(created);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new
				{
					message = ex.Message
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new
				{
					message = ex.Message
				});
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new
				{
					message = ex.Message
				});
			}
		}


		[HttpPut("{id}/working-hours/{workingHourId}")]
		public async Task<IActionResult> UpdateWorkingHour(
			string id,
			Guid workingHourId,
			[FromBody] UpdateEmployeeWorkingHourRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var updated =
					await _employeeService.UpdateWorkingHourAsync(
						id,
						workingHourId,
						request);

				return Ok(updated);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new
				{
					message = ex.Message
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new
				{
					message = ex.Message
				});
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new
				{
					message = ex.Message
				});
			}
		}


		[HttpDelete("{id}/working-hours/{workingHourId}")]
		public async Task<IActionResult> DeleteWorkingHour(
			string id,
			Guid workingHourId)
		{
			try
			{
				var success =
					await _employeeService.DeleteWorkingHourAsync(
						id,
						workingHourId);

				if (!success)
					return NotFound();

				return NoContent();
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new
				{
					message = ex.Message
				});
			}
		}


		// ============================================================
		// LEAVE
		// ============================================================

		[HttpGet("{id}/leaves")]
		public async Task<IActionResult> GetLeaves(string id)
		{
			var leaves =
				await _employeeService.GetLeavesAsync(id);

			return Ok(leaves);
		}


		[HttpPost("{id}/leaves")]
		public async Task<IActionResult> AddLeave(
			string id,
			[FromBody] CreateEmployeeLeaveRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var created =
					await _employeeService.AddLeaveAsync(
						id,
						request);

				return Ok(created);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new
				{
					message = ex.Message
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new
				{
					message = ex.Message
				});
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new
				{
					message = ex.Message
				});
			}
		}


		[HttpPut("{id}/leaves/{leaveId}")]
		public async Task<IActionResult> UpdateLeave(
			string id,
			Guid leaveId,
			[FromBody] UpdateEmployeeLeaveRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var updated =
					await _employeeService.UpdateLeaveAsync(
						id,
						leaveId,
						request);

				return Ok(updated);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new
				{
					message = ex.Message
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new
				{
					message = ex.Message
				});
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new
				{
					message = ex.Message
				});
			}
		}


		[HttpDelete("{id}/leaves/{leaveId}")]
		public async Task<IActionResult> DeleteLeave(
			string id,
			Guid leaveId)
		{
			try
			{
				var success =
					await _employeeService.DeleteLeaveAsync(
						id,
						leaveId);

				if (!success)
					return NotFound();

				return NoContent();
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new
				{
					message = ex.Message
				});
			}
		}



	}
}

