using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface ITicketPriorityService
    {
        Task<PagedResponse<TicketPriorityResponse>> GetAllAsync(TicketMasterQueryParams queryParams);
        Task<TicketPriorityResponse?> GetByIdAsync(Guid id);
        Task<TicketPriorityResponse> CreateAsync(CreateTicketPriorityRequest request, string actorUserId);
        Task<TicketPriorityResponse?> UpdateAsync(Guid id, UpdateTicketPriorityRequest request, string actorUserId);
		Task<bool> SoftDeleteAsync(Guid id, string actorUserId);
        Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId);
    }
}
