using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.HolidayRequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HolidaysController : ControllerBase
	{
		private readonly IHolidayService _holidayService;

		public HolidaysController(IHolidayService holidayService)
		{
			_holidayService = holidayService;
		}

		// ============================================================
		// ADMIN: CALENDAR CRUD
		// ============================================================

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var holidays = await _holidayService.GetAllAsync();
			return Ok(holidays);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var holiday = await _holidayService.GetByIdAsync(id);
			if (holiday == null) return NotFound();
			return Ok(holiday);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateHolidayRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var created = await _holidayService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHolidayRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var updated = await _holidayService.UpdateAsync(id, request);
				return Ok(updated);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var success = await _holidayService.SoftDeleteAsync(id);
			if (!success) return NotFound();
			return NoContent();
		}

		// ============================================================
		// EMPLOYEE: CONFIRMATIONS
		// ============================================================

		// GET /api/holidays/employees/{employeeId}/confirmations
		[HttpGet("employees/{employeeId}/confirmations")]
		public async Task<IActionResult> GetConfirmationsForEmployee(string employeeId)
		{
			var confirmations = await _holidayService.GetConfirmationsForEmployeeAsync(employeeId);
			return Ok(confirmations);
		}

		// POST /api/holidays/{holidayId}/confirmations/{employeeId}
		[HttpPost("{holidayId}/confirmations/{employeeId}")]
		public async Task<IActionResult> Confirm(
			Guid holidayId,
			string employeeId,
			[FromBody] ConfirmHolidayRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var result = await _holidayService.ConfirmAsync(employeeId, holidayId, request);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
		}
	}
}