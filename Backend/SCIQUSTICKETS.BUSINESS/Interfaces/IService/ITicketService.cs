using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface ITicketService
	{
		Task<PagedResponse<TicketResponse>> GetAllAsync(
			TicketQueryParams queryParams,
			string? userId = null,
			bool canViewAll = true,
			bool isCustomer = false);
		Task<TicketResponse?> GetByIdAsync(Guid ticketId);

		Task<TicketResponse> CreateAsync(
			string userId,
			CreateTicketRequest request);

		Task<TicketResponse> UpdateAsync(Guid ticketId, UpdateTicketRequest request, string actorUserId);

		Task<bool> ChangeStatusAsync(
			Guid ticketId,
			ChangeTicketStatusRequest request,
			string userId);

		Task<bool> AddCommentAsync(
			Guid ticketId,
			AddTicketCommentRequest request,
			string userId);

		Task<bool> DeleteCommentAsync(Guid ticketId, Guid commentId, string actorUserId, bool canManageAll);

		Task<bool> SoftDeleteAsync(Guid ticketId, string actorUserId = "SYSTEM");
		Task<bool> ReassignAsync(Guid ticketId, AssignTicketRequest request, string actorUserId);
		Task<bool> TransferDepartmentAsync(Guid ticketId, TransferTicketDepartmentRequest request, string actorUserId);
		Task<bool> ChangePriorityImpactAsync(Guid ticketId, ChangePriorityImpactRequest request, string actorUserId);
		Task<bool> ReopenAsync(Guid ticketId, string reason, string actorUserId, bool isAgent);
		Task<PagedResponse<TicketResponse>> GetMyQueueAsync(string userId, TicketQueryParams queryParams);
		Task<PagedResponse<TicketResponse>> GetDepartmentQueueAsync(string userId, TicketQueryParams queryParams);

		Task<bool> ConfirmClosureAsync(Guid ticketId, string accountActorId);
		Task<bool> RejectClosureAsync(Guid ticketId, string reason, string accountActorId);
		Task<TicketAttachmentResponse> UploadAttachmentAsync(Guid ticketId, IFormFile file, string actorUserId);
		Task<IEnumerable<TicketAttachmentResponse>> GetAttachmentsAsync(Guid ticketId);
		Task<IEnumerable<TicketCommentResponse>> GetCommentsAsync(Guid ticketId);
		Task<bool> DeleteAttachmentAsync(Guid ticketId, Guid attachmentId, string actorUserId, bool canManageAll);

		Task<AssignmentExplanationResponse?> GetAssignmentExplanationAsync(Guid ticketId);
		Task<string?> GetStatusNameAsync(Guid statusId);
	}
}