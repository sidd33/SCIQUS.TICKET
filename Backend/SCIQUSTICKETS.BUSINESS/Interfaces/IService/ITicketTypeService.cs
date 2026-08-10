using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface ITicketTypeService
    {
        Task<PagedResponse<TicketTypeResponse>> GetAllAsync(TicketMasterQueryParams queryParams);
        Task<TicketTypeResponse?> GetByIdAsync(Guid id);
        Task<TicketTypeResponse> CreateAsync(CreateTicketTypeRequest request, string actorUserId);
        Task<TicketTypeResponse?> UpdateAsync(Guid id, UpdateTicketTypeRequest request, string actorUserId);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId);
    }
}
