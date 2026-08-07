using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class TicketPriorityService : ITicketPriorityService
    {
        private readonly ITicketPriorityRepository _ticketPriorityRepository;

        public TicketPriorityService(ITicketPriorityRepository ticketPriorityRepository)
        {
            _ticketPriorityRepository = ticketPriorityRepository;
        }

        public async Task<PagedResponse<TicketPriorityResponse>> GetAllAsync(TicketMasterQueryParams queryParams)
        {
            var (items, totalCount) = await _ticketPriorityRepository.GetAllPagedAsync(
                queryParams.IncludeInactive,
                queryParams.Search,
                queryParams.SortBy,
                queryParams.SortDescending,
                queryParams.Page,
                queryParams.PageSize);

            return new PagedResponse<TicketPriorityResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<TicketPriorityResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _ticketPriorityRepository.GetByIdAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<TicketPriorityResponse> CreateAsync(CreateTicketPriorityRequest request, string actorUserId)
        {
            if (request.SlaInHours < 0)
                throw new InvalidOperationException("SlaInHours must be >= 0.");

            if (await _ticketPriorityRepository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A Ticket Priority named '{request.Name}' already exists.");

            var now = TimeHelper.GetIndianTime();

            var entity = new TicketPriority
            {
                Name = request.Name.Trim(),
                Level = request.Level,
                SlaInHours = request.SlaInHours,
                Status = true,
                IsDeleted = false,
                CreatedDate = now,
                LastUpdatedDate = now,
                CreatedByUserId = actorUserId,
                LastUpdatedByUserId = actorUserId
            };

            await _ticketPriorityRepository.AddAsync(entity);
            await _ticketPriorityRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<TicketPriorityResponse?> UpdateAsync(Guid id, UpdateTicketPriorityRequest request, string actorUserId)
        {
            var entity = await _ticketPriorityRepository.GetByIdAsync(id);
            if (entity == null) return null;

            if (request.SlaInHours < 0)
                throw new InvalidOperationException("SlaInHours must be >= 0.");

            if (await _ticketPriorityRepository.ExistsByNameAsync(request.Name, id))
                throw new InvalidOperationException($"A Ticket Priority named '{request.Name}' already exists.");

            if (entity.Status && !request.Status && await _ticketPriorityRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("Cannot deactivate: this Priority is in use by open tickets.");

            entity.Name = request.Name.Trim();
            entity.Level = request.Level;
            entity.SlaInHours = request.SlaInHours;
            entity.Status = request.Status;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
            entity.LastUpdatedByUserId = actorUserId;

            _ticketPriorityRepository.Update(entity);
            await _ticketPriorityRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

		public async Task<bool> SoftDeleteAsync(Guid id, string actorUserId)
		{
            var entity = await _ticketPriorityRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (await _ticketPriorityRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("This Priority is in use by open tickets and cannot be deleted.");

            entity.IsDeleted = true;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
			entity.LastUpdatedByUserId = actorUserId;

			_ticketPriorityRepository.Update(entity);
            await _ticketPriorityRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId)
        {
            var entity = await _ticketPriorityRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (entity.Status && !status && await _ticketPriorityRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("Cannot deactivate: this Priority is in use by open tickets.");

            entity.Status = status;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
            entity.LastUpdatedByUserId = actorUserId;

            _ticketPriorityRepository.Update(entity);
            await _ticketPriorityRepository.SaveChangesAsync();
            return true;
        }

        private static TicketPriorityResponse MapToResponse(TicketPriority p)
        {
            return new TicketPriorityResponse
            {
                TicketPriorityId = p.TicketPriorityId,
                Name = p.Name,
                Level = p.Level,
                SlaInHours = p.SlaInHours,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                LastUpdatedDate = p.LastUpdatedDate
            };
        }
    }
}
