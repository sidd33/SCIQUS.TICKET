using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class TicketController : ControllerBase
	{
		private readonly ITicketService _ticketService;

		public TicketController(ITicketService ticketService)
		{
			_ticketService = ticketService;
		}


		// POST: api/tickets
		[HttpPost]
		public async Task<ActionResult<TicketResponse>> Create(
			[FromBody] CreateTicketRequest request)
		{
			var userId = GetUserId();

			var result = await _ticketService.CreateAsync(
				userId,
				request);

			return Ok(result);
		}



		// GET: api/tickets
		[HttpGet]
		public async Task<ActionResult<PagedResponse<TicketResponse>>> GetAll(
			[FromQuery] TicketQueryParams queryParams)
		{
			var result = await _ticketService.GetAllAsync(queryParams);

			return Ok(result);
		}



		// GET: api/tickets/{id}
		[HttpGet("{id:guid}")]
		public async Task<ActionResult<TicketResponse>> GetById(
			Guid id)
		{
			var result = await _ticketService.GetByIdAsync(id);

			if (result == null)
				return NotFound();

			return Ok(result);
		}



		// PUT: api/tickets/{id}
		[HttpPut("{id:guid}")]
		public async Task<ActionResult<TicketResponse>> Update(
			Guid id,
			[FromBody] UpdateTicketRequest request)
		{
			var result = await _ticketService.UpdateAsync(
				id,
				request);

			return Ok(result);
		}



		// PATCH: api/tickets/{id}/status
		[HttpPatch("{id:guid}/status")]
		public async Task<IActionResult> ChangeStatus(
			Guid id,
			[FromBody] ChangeTicketStatusRequest request)
		{
			var userId = GetUserId();

			var result = await _ticketService.ChangeStatusAsync(
				id,
				request,
				userId);

			if (!result)
				return BadRequest("Unable to change ticket status.");

			return Ok(new
			{
				Message = "Ticket status updated successfully."
			});
		}



		// POST: api/tickets/{id}/comments
		[HttpPost("{id:guid}/comments")]
		public async Task<IActionResult> AddComment(
			Guid id,
			[FromBody] AddTicketCommentRequest request)
		{
			var userId = GetUserId();

			var result = await _ticketService.AddCommentAsync(
				id,
				request,
				userId);

			if (!result)
				return BadRequest("Unable to add comment.");

			return Ok(new
			{
				Message = "Comment added successfully."
			});
		}



		// DELETE: api/tickets/{id}
		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _ticketService.SoftDeleteAsync(id);

			if (!result)
				return NotFound();

			return Ok(new
			{
				Message = "Ticket deleted successfully."
			});
		}



		private string GetUserId()
		{
			return User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException(
					"User id not found in token.");
		}
	}
}