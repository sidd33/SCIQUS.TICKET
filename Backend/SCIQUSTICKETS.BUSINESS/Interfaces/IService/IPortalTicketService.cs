// SCIQUSTICKETS.BUSINESS/Interfaces/IService/IPortalTicketService.cs
using Microsoft.AspNetCore.Http;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IPortalTicketService
	{
		Task<PortalTicketResponse> CreateAsync(string accountId, PortalCreateTicketRequest request);
		Task<PagedResponse<PortalTicketResponse>> GetMyTicketsAsync(string accountId, TicketQueryParams queryParams);
		Task<PortalTicketDetailResponse?> GetByIdAsync(string accountId, Guid ticketId);
		Task<bool> AddCommentAsync(string accountId, Guid ticketId, PortalAddCommentRequest request);
		Task<TicketAttachmentResponse?> UploadAttachmentAsync(string accountId, Guid ticketId, IFormFile file);
		Task<bool?> ConfirmClosureAsync(string accountId, Guid ticketId);
		Task<bool?> RejectClosureAsync(string accountId, Guid ticketId, string reason);
		// IPortalTicketService.cs — add
		Task<PortalTicketResponse> CreateFollowUpAsync(string accountId, Guid parentTicketId, PortalCreateTicketRequest request);
		Task<bool?> ReopenAsync(string accountId, Guid ticketId, string reason);
	}
}