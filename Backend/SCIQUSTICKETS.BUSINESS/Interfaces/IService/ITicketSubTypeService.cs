using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface ITicketSubTypeService
    {
        Task<PagedResponse<TicketSubTypeResponse>> GetAllAsync(TicketSubTypeQueryParams queryParams);
        Task<TicketSubTypeResponse?> GetByIdAsync(Guid id);
        Task<TicketSubTypeResponse> CreateAsync(CreateTicketSubTypeRequest request, string actorUserId);
        Task<TicketSubTypeResponse?> UpdateAsync(Guid id, UpdateTicketSubTypeRequest request, string actorUserId);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId);
    }
}
