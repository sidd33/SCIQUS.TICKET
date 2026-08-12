// SCIQUSTICKETS.BUSINESS/Implementations/Service/PortalTicketService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class PortalTicketService : IPortalTicketService
	{
		private readonly ITicketService _ticketService;
		private readonly AppDbContext _context;

		public PortalTicketService(ITicketService ticketService, AppDbContext context)
		{
			_ticketService = ticketService;
			_context = context;
		}

		private async Task<bool> IsValidAccountAsync(string accountId)
			=> await _context.Accounts.AsNoTracking().AnyAsync(a => a.AccountId == accountId && !a.IsDeleted);

		public async Task<PortalTicketResponse> CreateAsync(string accountId, PortalCreateTicketRequest request)
		{
			if (!await IsValidAccountAsync(accountId))
				throw new UnauthorizedAccessException("Account not found.");

			var createRequest = new CreateTicketRequest
			{
				Title = request.Title,
				Description = request.Description,
				AccountId = accountId,
				RaisedByEmployeeId = null,
				IsInternal = false,
				SourceType = "Portal",
				TicketTypeId = request.TicketTypeId,
				TicketSubTypeId = request.TicketSubTypeId,
				PriorityId = request.PriorityId,
				BusinessImpactId = request.BusinessImpactId,
				DepartmentId = null,
				AssignedToUserId = null
			};

			var full = await _ticketService.CreateAsync(accountId, createRequest);
			return MapToPortalResponse(full);
		}

		public async Task<PagedResponse<PortalTicketResponse>> GetMyTicketsAsync(string accountId, TicketQueryParams queryParams)
		{
			// Ownership is enforced here, server-side — never trust a client-supplied AccountId.
			queryParams.AccountId = accountId;

			var result = await _ticketService.GetAllAsync(queryParams);

			return new PagedResponse<PortalTicketResponse>
			{
				Items = result.Items.Select(MapToPortalResponse).ToList(),
				TotalCount = result.TotalCount,
				Page = result.Page,
				PageSize = result.PageSize
			};
		}

		public async Task<PortalTicketDetailResponse?> GetByIdAsync(string accountId, Guid ticketId)
		{
			var ticket = await _ticketService.GetByIdAsync(ticketId);
			if (ticket == null || ticket.AccountId != accountId) return null; // 404, not 403 — doesn't reveal existence

			var comments = await _ticketService.GetCommentsAsync(ticketId);
			var customerVisibleComments = comments.Where(c => !c.IsInternalNote).ToList();

			var attachments = await _ticketService.GetAttachmentsAsync(ticketId);

			var detail = MapToPortalResponse(ticket) is PortalTicketResponse baseResp
				? new PortalTicketDetailResponse
				{
					TicketId = baseResp.TicketId,
					TicketNumber = baseResp.TicketNumber,
					Title = baseResp.Title,
					Description = baseResp.Description,
					TicketTypeName = baseResp.TicketTypeName,
					TicketSubTypeName = baseResp.TicketSubTypeName,
					PriorityName = baseResp.PriorityName,
					BusinessImpactName = baseResp.BusinessImpactName,
					StatusName = baseResp.StatusName,
					IsOpen = baseResp.IsOpen,
					SlaDueDate = baseResp.SlaDueDate,
					SlaMetStatus = baseResp.SlaMetStatus,
					CreatedDate = baseResp.CreatedDate,
					LastUpdatedDate = baseResp.LastUpdatedDate,
					Comments = customerVisibleComments.ToList(),
					Attachments = attachments.ToList()
				}
				: null;

			return detail;
		}

		public async Task<bool> AddCommentAsync(string accountId, Guid ticketId, PortalAddCommentRequest request)
		{
			var ticket = await _ticketService.GetByIdAsync(ticketId);
			if (ticket == null || ticket.AccountId != accountId) return false;

			var addRequest = new AddTicketCommentRequest
			{
				Comment = request.Comment,
				IsInternalNote = false // customers can never post internal notes
			};

			return await _ticketService.AddCommentAsync(ticketId, addRequest, accountId);
		}

		public async Task<TicketAttachmentResponse?> UploadAttachmentAsync(string accountId, Guid ticketId, IFormFile file)
		{
			var ticket = await _ticketService.GetByIdAsync(ticketId);
			if (ticket == null || ticket.AccountId != accountId) return null;

			return await _ticketService.UploadAttachmentAsync(ticketId, file, accountId);
		}

		public async Task<bool?> ConfirmClosureAsync(string accountId, Guid ticketId)
		{
			var ticket = await _ticketService.GetByIdAsync(ticketId);
			if (ticket == null || ticket.AccountId != accountId) return null;

			return await _ticketService.ConfirmClosureAsync(ticketId, accountId);
		}

		public async Task<bool?> RejectClosureAsync(string accountId, Guid ticketId, string reason)
		{
			var ticket = await _ticketService.GetByIdAsync(ticketId);
			if (ticket == null || ticket.AccountId != accountId) return null;

			return await _ticketService.RejectClosureAsync(ticketId, reason, accountId);
		}

		public async Task<bool?> ReopenAsync(string accountId, Guid ticketId, string reason)
		{
			var ticket = await _ticketService.GetByIdAsync(ticketId);
			if (ticket == null || ticket.AccountId != accountId) return null;

			// isAgent: false — ReopenAsync already enforces AllowEmployeeReopen + ReopenGraceDays for non-agents.
			return await _ticketService.ReopenAsync(ticketId, reason, accountId, isAgent: false);
		}

		// PortalTicketService.cs — add
		public async Task<PortalTicketResponse> CreateFollowUpAsync(string accountId, Guid parentTicketId, PortalCreateTicketRequest request)
		{
			var parent = await _ticketService.GetByIdAsync(parentTicketId);
			if (parent == null || parent.AccountId != accountId)
				throw new UnauthorizedAccessException("Original ticket not found.");

			var result = await CreateAsync(accountId, request);

			var newTicketEntity = await _context.Tickets.FirstAsync(t => t.TicketId == result.TicketId);
			newTicketEntity.ParentTicketId = parentTicketId;
			await _context.SaveChangesAsync();

			return result;
		}

		private static PortalTicketResponse MapToPortalResponse(TicketResponse t) => new()
		{
			TicketId = t.TicketId,
			TicketNumber = t.TicketNumber,
			Title = t.Title,
			Description = t.Description,
			TicketTypeName = t.TicketTypeName,
			TicketSubTypeName = t.TicketSubTypeName,
			PriorityName = t.PriorityName,
			BusinessImpactName = t.BusinessImpactName,
			StatusName = t.StatusName,
			IsOpen = t.IsOpen,
			SlaDueDate = t.SlaDueDate,
			SlaMetStatus = t.SlaMetStatus,
			CreatedDate = t.CreatedDate,
			LastUpdatedDate = t.LastUpdatedDate
		};
	}
}