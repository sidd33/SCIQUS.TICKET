using System;
using System.Collections.Generic;
using System.Text;
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
	        TicketQueryParams queryParams);

		Task<TicketResponse?> GetByIdAsync(Guid ticketId);

		Task<TicketResponse> CreateAsync(
			string userId,
			CreateTicketRequest request);

		Task<TicketResponse> UpdateAsync(
			Guid ticketId,
			UpdateTicketRequest request);

		Task<bool> ChangeStatusAsync(
			Guid ticketId,
			ChangeTicketStatusRequest request,
			string userId);

		Task<bool> AddCommentAsync(
			Guid ticketId,
			AddTicketCommentRequest request,
			string userId);

		Task<bool> SoftDeleteAsync(Guid ticketId);
	}
}
