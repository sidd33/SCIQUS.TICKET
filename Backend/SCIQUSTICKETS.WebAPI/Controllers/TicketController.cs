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

			try
			{
				var result = await _ticketService.CreateAsync(userId, request);
				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
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
			var userId = GetUserId();

			try
			{
				var result = await _ticketService.UpdateAsync(id, request, userId);
				return Ok(result);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}



		// PATCH: api/tickets/{id}/status
		[HttpPatch("{id:guid}/status")]
		public async Task<IActionResult> ChangeStatus(
			Guid id,
			[FromBody] ChangeTicketStatusRequest request)
		{
			var userId = GetUserId();

			try
			{
				var result = await _ticketService.ChangeStatusAsync(id, request, userId);

				if (!result)
					return NotFound();

				return Ok(new { Message = "Ticket status updated successfully." });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}



		// POST: api/tickets/{id}/comments
		[HttpPost("{id:guid}/comments")]
		public async Task<IActionResult> AddComment(
			Guid id,
			[FromBody] AddTicketCommentRequest request)
		{
			var userId = GetUserId();

			var result = await _ticketService.AddCommentAsync(id, request, userId);

			if (!result)
				return NotFound();

			return Ok(new { Message = "Comment added successfully." });
		}



		// DELETE: api/tickets/{id}/comments/{commentId}
		[HttpDelete("{id:guid}/comments/{commentId:guid}")]
		public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
		{
			var userId = GetUserId();

			// TODO: replace with a real ticket.manage.all policy check once policies are wired
			try
			{
				var result = await _ticketService.DeleteCommentAsync(id, commentId, userId, canManageAll: false);
				if (!result) return NotFound();

				return Ok(new { Message = "Comment deleted successfully." });
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}



		// POST: api/tickets/{id}/attachments
		[HttpPost("{id:guid}/attachments")]
		public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
		{
			var userId = GetUserId();

			try
			{
				var result = await _ticketService.UploadAttachmentAsync(id, file, userId);
				return Ok(result);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}



		// GET: api/tickets/{id}/attachments
		[HttpGet("{id:guid}/attachments")]
		public async Task<IActionResult> GetAttachments(Guid id)
		{
			var result = await _ticketService.GetAttachmentsAsync(id);
			return Ok(result);
		}



		// DELETE: api/tickets/{id}/attachments/{attachmentId}
		[HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
		public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
		{
			var userId = GetUserId();

			// TODO: replace with a real ticket.manage.all policy check once policies are wired
			try
			{
				var result = await _ticketService.DeleteAttachmentAsync(id, attachmentId, userId, canManageAll: false);
				if (!result) return NotFound();

				return Ok(new { Message = "Attachment deleted successfully." });
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
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



		// POST: api/tickets/{id}/reassign
		[HttpPost("{id:guid}/reassign")]
		public async Task<IActionResult> Reassign(Guid id, [FromBody] AssignTicketRequest request)
		{
			var userId = GetUserId();
			try
			{
				var result = await _ticketService.ReassignAsync(id, request, userId);
				if (!result) return NotFound();
				return Ok(new { Message = "Ticket reassigned successfully." });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}



		// POST: api/tickets/{id}/transfer
		[HttpPost("{id:guid}/transfer")]
		public async Task<IActionResult> TransferDepartment(Guid id, [FromBody] TransferTicketDepartmentRequest request)
		{
			var userId = GetUserId();
			try
			{
				var result = await _ticketService.TransferDepartmentAsync(id, request, userId);
				if (!result) return NotFound();
				return Ok(new { Message = "Ticket transferred successfully." });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}



		// PATCH: api/tickets/{id}/priority-impact
		[HttpPatch("{id:guid}/priority-impact")]
		public async Task<IActionResult> ChangePriorityImpact(Guid id, [FromBody] ChangePriorityImpactRequest request)
		{
			var userId = GetUserId();
			try
			{
				var result = await _ticketService.ChangePriorityImpactAsync(id, request, userId);
				if (!result) return NotFound();
				return Ok(new { Message = "Priority/Impact changed and SLA recalculated successfully." });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}



		// GET: api/tickets/my-queue
		[HttpGet("my-queue")]
		public async Task<ActionResult<PagedResponse<TicketResponse>>> GetMyQueue([FromQuery] TicketQueryParams queryParams)
		{
			var userId = GetUserId();
			var result = await _ticketService.GetMyQueueAsync(userId, queryParams);
			return Ok(result);
		}



		// GET: api/tickets/department-queue
		[HttpGet("department-queue")]
		public async Task<ActionResult<PagedResponse<TicketResponse>>> GetDepartmentQueue([FromQuery] TicketQueryParams queryParams)
		{
			var userId = GetUserId();
			try
			{
				var result = await _ticketService.GetDepartmentQueueAsync(userId, queryParams);
				return Ok(result);
			}
			catch (UnauthorizedAccessException ex)
			{
				return Unauthorized(new { message = ex.Message });
			}
		}



		private string GetUserId()
		{
			return User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException(
					"User id not found in token.");
		}
	}
}