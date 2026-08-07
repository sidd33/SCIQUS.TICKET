using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface ITicketBusinessImpactService
    {
        Task<PagedResponse<TicketBusinessImpactResponse>> GetAllAsync(TicketMasterQueryParams queryParams);
        Task<TicketBusinessImpactResponse?> GetByIdAsync(Guid id);
        Task<TicketBusinessImpactResponse> CreateAsync(CreateTicketBusinessImpactRequest request, string actorUserId);
        Task<TicketBusinessImpactResponse?> UpdateAsync(Guid id, UpdateTicketBusinessImpactRequest request, string actorUserId);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId);
    }
}
