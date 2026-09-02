using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using System.Security.Claims;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/TicketTypes")]
	[Authorize]
	public class TicketTypesController : ControllerBase
	{
		private readonly ITicketTypeService _ticketTypeService;

		public TicketTypesController(ITicketTypeService ticketTypeService)
		{
			_ticketTypeService = ticketTypeService;
		}

		[HttpGet]
		[Authorize(Policy = "ticketmaster.view")]
		public async Task<IActionResult> GetAll([FromQuery] TicketMasterQueryParams queryParams)
		{
			var result = await _ticketTypeService.GetAllAsync(queryParams);
			return Ok(result);
		}

		[HttpGet("{id}")]
		[Authorize(Policy = "ticketmaster.view")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var result = await _ticketTypeService.GetByIdAsync(id);
			if (result == null) return NotFound(new { message = $"Ticket Type with ID {id} not found." });

			return Ok(result);
		}

		[HttpPost]
		[Authorize(Policy = "ticketmaster.manage")]
		public async Task<IActionResult> Create([FromBody] CreateTicketTypeRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

			try
			{
				var created = await _ticketTypeService.CreateAsync(request, actorUserId);
				return CreatedAtAction(nameof(GetById), new { id = created.TicketTypeId }, created);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPut("{id}")]
		[Authorize(Policy = "ticketmaster.manage")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTicketTypeRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

			try
			{
				var updated = await _ticketTypeService.UpdateAsync(id, request, actorUserId);
				if (updated == null) return NotFound(new { message = $"Ticket Type with ID {id} not found." });

				return Ok(updated);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPatch("{id}/status")]
		[Authorize(Policy = "ticketmaster.manage")]
		public async Task<IActionResult> SetStatus(Guid id, [FromQuery] bool status)
		{
			var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

			try
			{
				var updated = await _ticketTypeService.SetStatusAsync(id, status, actorUserId);
				if (!updated) return NotFound(new { message = $"Ticket Type with ID {id} not found." });

				return Ok(new { message = $"Ticket Type {id} status set to {status}." });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Policy = "ticketmaster.manage")]
		public async Task<IActionResult> SoftDelete(Guid id)
		{
			try
			{
				var deleted = await _ticketTypeService.SoftDeleteAsync(id);
				if (!deleted) return NotFound(new { message = $"Ticket Type with ID {id} not found." });

				return Ok(new { message = $"Ticket Type {id} soft-deleted successfully." });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}