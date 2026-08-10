using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class TicketBusinessImpactService : ITicketBusinessImpactService
    {
        private readonly ITicketBusinessImpactRepository _ticketBusinessImpactRepository;

        public TicketBusinessImpactService(ITicketBusinessImpactRepository ticketBusinessImpactRepository)
        {
            _ticketBusinessImpactRepository = ticketBusinessImpactRepository;
        }

        public async Task<PagedResponse<TicketBusinessImpactResponse>> GetAllAsync(TicketMasterQueryParams queryParams)
        {
            var (items, totalCount) = await _ticketBusinessImpactRepository.GetAllPagedAsync(
                queryParams.IncludeInactive,
                queryParams.Search,
                queryParams.SortBy,
                queryParams.SortDescending,
                queryParams.Page,
                queryParams.PageSize);

            return new PagedResponse<TicketBusinessImpactResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<TicketBusinessImpactResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _ticketBusinessImpactRepository.GetByIdAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<TicketBusinessImpactResponse> CreateAsync(CreateTicketBusinessImpactRequest request, string actorUserId)
        {
            if (await _ticketBusinessImpactRepository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A Business Impact named '{request.Name}' already exists.");

            var now = TimeHelper.GetIndianTime();

            var entity = new TicketBusinessTypeImpact
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                Status = true,
                IsDeleted = false,
                CreatedDate = now,
                LastUpdatedDate = now,
                CreatedByUserId = actorUserId,
                LastUpdatedByUserId = actorUserId
            };

            await _ticketBusinessImpactRepository.AddAsync(entity);
            await _ticketBusinessImpactRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<TicketBusinessImpactResponse?> UpdateAsync(Guid id, UpdateTicketBusinessImpactRequest request, string actorUserId)
        {
            var entity = await _ticketBusinessImpactRepository.GetByIdAsync(id);
            if (entity == null) return null;

            if (await _ticketBusinessImpactRepository.ExistsByNameAsync(request.Name, id))
                throw new InvalidOperationException($"A Business Impact named '{request.Name}' already exists.");

            if (entity.Status && !request.Status && await _ticketBusinessImpactRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("Cannot deactivate: this Business Impact is in use by open tickets.");

            entity.Name = request.Name.Trim();
            entity.Description = request.Description;
            entity.Status = request.Status;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
            entity.LastUpdatedByUserId = actorUserId;

            _ticketBusinessImpactRepository.Update(entity);
            await _ticketBusinessImpactRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var entity = await _ticketBusinessImpactRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (await _ticketBusinessImpactRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("This Business Impact is in use by open tickets and cannot be deleted.");

            entity.IsDeleted = true;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();

            _ticketBusinessImpactRepository.Update(entity);
            await _ticketBusinessImpactRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId)
        {
            var entity = await _ticketBusinessImpactRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (entity.Status && !status && await _ticketBusinessImpactRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("Cannot deactivate: this Business Impact is in use by open tickets.");

            entity.Status = status;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
            entity.LastUpdatedByUserId = actorUserId;

            _ticketBusinessImpactRepository.Update(entity);
            await _ticketBusinessImpactRepository.SaveChangesAsync();
            return true;
        }

        private static TicketBusinessImpactResponse MapToResponse(TicketBusinessTypeImpact i)
        {
            return new TicketBusinessImpactResponse
            {
                TicketBusinessTypeImpactId = i.TicketBusinessTypeImpactId,
                Name = i.Name,
                Description = i.Description,
                Status = i.Status,
                CreatedDate = i.CreatedDate,
                LastUpdatedDate = i.LastUpdatedDate
            };
        }
    }
}
