using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class TicketTypeService : ITicketTypeService
    {
        private readonly ITicketTypeRepository _ticketTypeRepository;

        public TicketTypeService(ITicketTypeRepository ticketTypeRepository)
        {
            _ticketTypeRepository = ticketTypeRepository;
        }

        public async Task<PagedResponse<TicketTypeResponse>> GetAllAsync(TicketMasterQueryParams queryParams)
        {
            var (items, totalCount) = await _ticketTypeRepository.GetAllPagedAsync(
                queryParams.IncludeInactive,
                queryParams.Search,
                queryParams.SortBy,
                queryParams.SortDescending,
                queryParams.Page,
                queryParams.PageSize);

            var responses = new List<TicketTypeResponse>();
            foreach (var t in items)
            {
                responses.Add(await MapToResponseAsync(t));
            }

            return new PagedResponse<TicketTypeResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<TicketTypeResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _ticketTypeRepository.GetByIdWithSubTypesAsync(id);
            if (entity == null) return null;
            return await MapToResponseAsync(entity);
        }

        public async Task<TicketTypeResponse> CreateAsync(CreateTicketTypeRequest request, string actorUserId)
        {
            if (await _ticketTypeRepository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A Ticket Type named '{request.Name}' already exists.");

            var now = TimeHelper.GetIndianTime();

            var entity = new TicketType
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

            await _ticketTypeRepository.AddAsync(entity);
            await _ticketTypeRepository.SaveChangesAsync();

            return await MapToResponseAsync(entity);
        }

        public async Task<TicketTypeResponse?> UpdateAsync(Guid id, UpdateTicketTypeRequest request, string actorUserId)
        {
            var entity = await _ticketTypeRepository.GetByIdAsync(id);
            if (entity == null) return null;

            if (await _ticketTypeRepository.ExistsByNameAsync(request.Name, id))
                throw new InvalidOperationException($"A Ticket Type named '{request.Name}' already exists.");

            // Deactivating: block while active Sub-Types still reference this Type.
            if (entity.Status && !request.Status)
            {
                var activeSubTypeCount = await _ticketTypeRepository.GetActiveSubTypeCountAsync(id);
                if (activeSubTypeCount > 0)
                    throw new InvalidOperationException(
                        $"Cannot deactivate this Ticket Type: {activeSubTypeCount} active Sub-Type(s) still use it.");
            }

            entity.Name = request.Name.Trim();
            entity.Description = request.Description;
            entity.Status = request.Status;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
            entity.LastUpdatedByUserId = actorUserId;

            _ticketTypeRepository.Update(entity);
            await _ticketTypeRepository.SaveChangesAsync();

            return await MapToResponseAsync(entity);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var entity = await _ticketTypeRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (await _ticketTypeRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("This Ticket Type is in use by open tickets and cannot be deleted.");

            var activeSubTypeCount = await _ticketTypeRepository.GetActiveSubTypeCountAsync(id);
            if (activeSubTypeCount > 0)
                throw new InvalidOperationException(
                    $"Cannot delete this Ticket Type: {activeSubTypeCount} active Sub-Type(s) still use it.");

            entity.IsDeleted = true;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();

            _ticketTypeRepository.Update(entity);
            await _ticketTypeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId)
        {
            var entity = await _ticketTypeRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (entity.Status && !status)
            {
                var activeSubTypeCount = await _ticketTypeRepository.GetActiveSubTypeCountAsync(id);
                if (activeSubTypeCount > 0)
                    throw new InvalidOperationException(
                        $"Cannot deactivate this Ticket Type: {activeSubTypeCount} active Sub-Type(s) still use it.");
            }

            entity.Status = status;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
            entity.LastUpdatedByUserId = actorUserId;

            _ticketTypeRepository.Update(entity);
            await _ticketTypeRepository.SaveChangesAsync();
            return true;
        }

        private async Task<TicketTypeResponse> MapToResponseAsync(TicketType t)
        {
            int subTypeCount = t.TicketSubTypes?.Count(st => !st.IsDeleted)
                ?? await _ticketTypeRepository.GetActiveSubTypeCountAsync(t.TicketTypeId);

            return new TicketTypeResponse
            {
                TicketTypeId = t.TicketTypeId,
                Name = t.Name,
                Description = t.Description,
                Status = t.Status,
                SubTypeCount = subTypeCount,
                CreatedDate = t.CreatedDate,
                LastUpdatedDate = t.LastUpdatedDate
            };
        }
    }
}
