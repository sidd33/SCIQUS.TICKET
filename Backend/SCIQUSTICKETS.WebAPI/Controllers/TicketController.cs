using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Implementations.Service;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/tickets")]
	[Authorize]
	public class TicketController : ControllerBase
	{
		private readonly ITicketService _ticketService;
		private readonly ITicketNotificationService _notificationService;
		private readonly IEmailChannelService _emailChannelService;
		private readonly IWhatsAppChannelService _whatsAppChannelService;
		private readonly IAcceptanceService _acceptanceService;
		private readonly IFaqArticleService _faqArticleService;
		private readonly ITicketTimelineService _timelineService;
		public TicketController(
			ITicketService ticketService,
			ITicketNotificationService notificationService,
			IEmailChannelService emailChannelService,
			IWhatsAppChannelService whatsAppChannelService,
			IAcceptanceService acceptanceService,
			IFaqArticleService faqArticleService,
			ITicketTimelineService timelineService)
		{
			_ticketService = ticketService;
			_notificationService = notificationService;
			_emailChannelService = emailChannelService;
			_whatsAppChannelService = whatsAppChannelService;
			_acceptanceService = acceptanceService;
			_faqArticleService = faqArticleService;
			_timelineService = timelineService;
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
				
				// Notify on ticket creation
				try { await _notificationService.NotifyTicketCreatedAsync(result.TicketId, userId); } catch { }

				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}



		// GET: api/Ticket/{id}/timeline
		[HttpGet("{id:guid}/timeline")]
		public async Task<ActionResult<List<TimelineEventResponse>>> GetTimeline(Guid id)
		{
			bool isInternalStaff = User.IsInRole("Admin") || User.IsInRole("Agent") || User.HasClaim(c => c.Type == "EmployeeId");
			
			var timeline = await _timelineService.GetTimelineAsync(id, excludeInternal: !isInternalStaff);
			return Ok(timeline);
		}

		// GET: api/tickets
		[HttpGet]
		public async Task<ActionResult<PagedResponse<TicketResponse>>> GetAll(
	[FromQuery] TicketQueryParams queryParams)
		{
			var userId = GetUserId();
			bool canViewAll = User.IsInRole("Admin");
			bool isCustomer = User.IsInRole("Customer");

			var result = await _ticketService.GetAllAsync(
				queryParams,
				userId,
				canViewAll,
				isCustomer);

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

				// Notify on status change
				try 
				{
					if (request.StatusId.ToString().EndsWith("5"))
						await _notificationService.NotifyClosedAsync(id, userId);
					else if (request.StatusId.ToString().EndsWith("6"))
						await _notificationService.NotifyPendingClosureAsync(id, userId);
					else if (request.StatusId.ToString().EndsWith("7"))
						await _notificationService.NotifyReopenedAsync(id, userId);
				} catch { }

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

			try { await _notificationService.NotifyCommentAddedAsync(id, userId, !request.IsInternalNote); } catch { }

			return Ok(new { Message = "Comment added successfully." });
		}



		// DELETE: api/tickets/{id}/comments/{commentId}
		[HttpDelete("{id:guid}/comments/{commentId:guid}")]
		public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
		{
			var userId = GetUserId();

			try
			{
				var result = await _ticketService.DeleteCommentAsync(
					id,
					commentId,
					userId,
					canManageAll: User.IsInRole("Admin")); if (!result) return NotFound();

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
			var userId = GetUserId();

			var result = await _ticketService.SoftDeleteAsync(id, userId);

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
				
				try { await _notificationService.NotifyAssignedAsync(id, userId); } catch { /* ignore */ }
				
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
				
				try { await _notificationService.NotifyTransferredAsync(id, userId); } catch { /* ignore */ }
				
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
				
				try { await _notificationService.NotifyPriorityChangedAsync(id, userId); } catch { /* ignore */ }
				
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

		// POST: api/tickets/{id}/email-reply
		[HttpPost("{id:guid}/email-reply")]
		public async Task<IActionResult> EmailReply(Guid id, [FromBody] string body)
		{
			var userId = GetUserId();
			var success = await _emailChannelService.SendOutboundReplyAsync(id, body, userId);
			if (!success) return BadRequest("Could not send email reply.");
			
			try { await _notificationService.NotifyCommentAddedAsync(id, userId, true); } catch { /* ignore */ }
			
			return Ok(new { Message = "Email reply sent successfully." });
		}

		// POST: api/tickets/{id}/whatsapp-reply
		public class WhatsAppReplyRequest { public string Body { get; set; } = null!; public string? TemplateName { get; set; } }

		[HttpPost("{id:guid}/whatsapp-reply")]
		public async Task<IActionResult> WhatsAppReply(Guid id, [FromBody] WhatsAppReplyRequest request)
		{
			var userId = GetUserId();
			var success = await _whatsAppChannelService.SendOutboundReplyAsync(id, request.Body, request.TemplateName, userId);
			if (!success) return BadRequest("Could not send WhatsApp reply.");
			
			try { await _notificationService.NotifyCommentAddedAsync(id, userId, true); } catch { /* ignore */ }
			
			return Ok(new { Message = "WhatsApp reply sent successfully." });
		}

		[HttpPost("{id:guid}/reopen")]
		public async Task<IActionResult> Reopen(Guid id, [FromBody] string reason)
		{
			var userId = GetUserId();
			try
			{
				var result = await _ticketService.ReopenAsync(id, reason, userId, isAgent: true);
				if (!result) return NotFound();

				try { await _notificationService.NotifyReopenedAsync(id, userId); } catch { }

				return Ok(new { Message = "Ticket reopened successfully." });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("{id:guid}/confirm-closure")]
		public async Task<IActionResult> ConfirmClosure(Guid id)
		{
			var userId = GetUserId();
			try
			{
				var result = await _ticketService.ConfirmClosureAsync(id, userId);
				if (!result) return NotFound();
				return Ok(new { Message = "Closure confirmed." });
			}
			catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
		}

		[HttpPost("{id:guid}/reject-closure")]
		public async Task<IActionResult> RejectClosure(Guid id, [FromBody] string reason)
		{
			var userId = GetUserId();
			try
			{
				var result = await _ticketService.RejectClosureAsync(id, reason, userId);
				if (!result) return NotFound();
				return Ok(new { Message = "Closure rejected, ticket reopened." });
			}
			catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
		}

		[HttpPost("{id:guid}/accept")]
		public async Task<IActionResult> AcceptTicket(Guid id)
		{
			var userId = GetUserId();
			try
			{
				var result = await _acceptanceService.AcceptAsync(id, userId);
				if (!result) return NotFound();
				return Ok(new { Message = "Ticket accepted." });
			}
			catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
		}

		[HttpPost("{id:guid}/reject")]
		public async Task<IActionResult> RejectTicket(Guid id, [FromBody] string reason)
		{
			var userId = GetUserId();
			try
			{
				var result = await _acceptanceService.RejectAsync(id, userId, reason);
				if (!result) return NotFound();
				return Ok(new { Message = "Ticket rejected; re-routed for fallback assignment." });
			}
			catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
		}
		[HttpGet("{id:guid}/comments")]
		public async Task<IActionResult> GetComments(Guid id)
		{
			var result = await _ticketService.GetCommentsAsync(id);
			return Ok(result);
		}

		[HttpGet("faq-suggestions")]
		public async Task<IActionResult> GetFaqSuggestions([FromQuery] Guid ticketTypeId, [FromQuery] Guid? subTypeId, [FromQuery] string? query)
		{
			var result = await _faqArticleService.GetSuggestionsAsync(ticketTypeId, subTypeId, query);
			return Ok(result);
		}



		private string GetUserId()
		{
			return User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException(
					"User id not found in token.");
		}
	}
}