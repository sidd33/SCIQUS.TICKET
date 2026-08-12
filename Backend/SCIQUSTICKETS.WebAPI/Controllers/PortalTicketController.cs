// SCIQUSTICKETS.WebAPI/Controllers/PortalTicketController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	// TODO: add [Authorize(Policy = "portalticket.access")] once wired.
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class PortalTicketController : ControllerBase
	{
		private readonly IPortalTicketService _portalTicketService;

		public PortalTicketController(IPortalTicketService portalTicketService)
		{
			_portalTicketService = portalTicketService;
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] PortalCreateTicketRequest request)
		{
			var accountId = GetAccountId();
			try
			{
				var result = await _portalTicketService.CreateAsync(accountId, request);
				return Ok(result);
			}
			catch (UnauthorizedAccessException ex)
			{
				return Unauthorized(new { message = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetMyTickets([FromQuery] TicketQueryParams queryParams)
		{
			var accountId = GetAccountId();
			var result = await _portalTicketService.GetMyTicketsAsync(accountId, queryParams);
			return Ok(result);
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var accountId = GetAccountId();
			var result = await _portalTicketService.GetByIdAsync(accountId, id);
			return result == null ? NotFound() : Ok(result);
		}

		[HttpPost("{id:guid}/comments")]
		public async Task<IActionResult> AddComment(Guid id, [FromBody] PortalAddCommentRequest request)
		{
			var accountId = GetAccountId();
			var result = await _portalTicketService.AddCommentAsync(accountId, id, request);
			return result ? Ok(new { Message = "Comment added." }) : NotFound();
		}

		[HttpPost("{id:guid}/attachments")]
		public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
		{
			var accountId = GetAccountId();
			var result = await _portalTicketService.UploadAttachmentAsync(accountId, id, file);
			return result == null ? NotFound() : Ok(result);
		}

		[HttpPost("{id:guid}/confirm-closure")]
		public async Task<IActionResult> ConfirmClosure(Guid id)
		{
			var accountId = GetAccountId();
			var result = await _portalTicketService.ConfirmClosureAsync(accountId, id);
			if (result == null) return NotFound();
			return Ok(new { Message = "Closure confirmed." });
		}

		[HttpPost("{id:guid}/reject-closure")]
		public async Task<IActionResult> RejectClosure(Guid id, [FromBody] string reason)
		{
			var accountId = GetAccountId();
			var result = await _portalTicketService.RejectClosureAsync(accountId, id, reason);
			if (result == null) return NotFound();
			return Ok(new { Message = "Closure rejected, ticket reopened." });
		}

		[HttpPost("{id:guid}/reopen")]
		public async Task<IActionResult> Reopen(Guid id, [FromBody] string reason)
		{
			var accountId = GetAccountId();
			try
			{
				var result = await _portalTicketService.ReopenAsync(accountId, id, reason);
				if (result == null) return NotFound();
				return Ok(new { Message = "Ticket reopened." });
			}
			catch (InvalidOperationException ex)
			{
				// e.g. outside grace window / reopen not allowed — doc: "clear message and a create-follow-up option"
				return BadRequest(new { message = ex.Message });
			}
		}

		// PortalTicketController.cs — add
		[HttpPost("{parentId:guid}/follow-up")]
		public async Task<IActionResult> CreateFollowUp(Guid parentId, [FromBody] PortalCreateTicketRequest request)
		{
			var accountId = GetAccountId();
			try
			{
				var result = await _portalTicketService.CreateFollowUpAsync(accountId, parentId, request);
				return Ok(result);
			}
			catch (UnauthorizedAccessException ex)
			{
				return Unauthorized(new { message = ex.Message });
			}
		}

		private string GetAccountId()
		{
			return User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("Account id not found in token.");
		}
	}
}