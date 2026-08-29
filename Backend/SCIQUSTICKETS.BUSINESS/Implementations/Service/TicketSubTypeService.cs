using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class TicketSubTypeService : ITicketSubTypeService
    {
        private readonly ITicketSubTypeRepository _ticketSubTypeRepository;

        public TicketSubTypeService(ITicketSubTypeRepository ticketSubTypeRepository)
        {
            _ticketSubTypeRepository = ticketSubTypeRepository;
        }

        public async Task<PagedResponse<TicketSubTypeResponse>> GetAllAsync(TicketSubTypeQueryParams queryParams)
        {
            var (items, totalCount) = await _ticketSubTypeRepository.GetAllPagedAsync(
                queryParams.IncludeInactive,
                queryParams.Search,
                queryParams.TicketTypeId,
                queryParams.DepartmentId,
                queryParams.SortBy,
                queryParams.SortDescending,
                queryParams.Page,
                queryParams.PageSize);

            return new PagedResponse<TicketSubTypeResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<TicketSubTypeResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _ticketSubTypeRepository.GetByIdWithDetailsAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<TicketSubTypeResponse> CreateAsync(CreateTicketSubTypeRequest request, string actorUserId)
        {
            await ValidateCascadeAsync(request.TicketTypeId, request.DepartmentId, request.DefaultUserId);

            if (await _ticketSubTypeRepository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A Ticket Sub-Type named '{request.Name}' already exists.");

            var now = TimeHelper.GetIndianTime();

			var entity = new TicketSubType
			{
				Name = request.Name.Trim(),
				Description = request.Description,
				TicketTypeId = request.TicketTypeId,
				DepartmentId = request.DepartmentId,
				DefaultUserId = request.DefaultUserId,

				RequiresAcceptance = request.RequiresAcceptance,
				AcceptanceDeadlineHours = request.AcceptanceDeadlineHours,
				ManualOnly = request.ManualOnly,

				Status = true,
				IsDeleted = false,
				CreatedDate = now,
				LastUpdatedDate = now,
				CreatedByUserId = actorUserId,
				LastUpdatedByUserId = actorUserId
			};

			await _ticketSubTypeRepository.AddAsync(entity);
            await _ticketSubTypeRepository.SaveChangesAsync();

            var created = await _ticketSubTypeRepository.GetByIdWithDetailsAsync(entity.TicketSubTypeId);
            return MapToResponse(created ?? entity);
        }

        public async Task<TicketSubTypeResponse?> UpdateAsync(Guid id, UpdateTicketSubTypeRequest request, string actorUserId)
        {
            var entity = await _ticketSubTypeRepository.GetByIdAsync(id);
            if (entity == null) return null;

            await ValidateCascadeAsync(request.TicketTypeId, request.DepartmentId, request.DefaultUserId);

            if (await _ticketSubTypeRepository.ExistsByNameAsync(request.Name, id))
                throw new InvalidOperationException($"A Ticket Sub-Type named '{request.Name}' already exists.");

            if (entity.Status && !request.Status && await _ticketSubTypeRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("Cannot deactivate: this Sub-Type is in use by open tickets.");

			entity.Name = request.Name.Trim();
			entity.Description = request.Description;
			entity.TicketTypeId = request.TicketTypeId;
			entity.DepartmentId = request.DepartmentId;
			entity.DefaultUserId = request.DefaultUserId;
			entity.RequiresAcceptance = request.RequiresAcceptance;
			entity.AcceptanceDeadlineHours = request.AcceptanceDeadlineHours;
			entity.ManualOnly = request.ManualOnly;
			entity.Status = request.Status;
			entity.LastUpdatedDate = TimeHelper.GetIndianTime();
			entity.LastUpdatedByUserId = actorUserId;

			_ticketSubTypeRepository.Update(entity);
            await _ticketSubTypeRepository.SaveChangesAsync();

            var updated = await _ticketSubTypeRepository.GetByIdWithDetailsAsync(id);
            return MapToResponse(updated ?? entity);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var entity = await _ticketSubTypeRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (await _ticketSubTypeRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("This Sub-Type is in use by open tickets and cannot be deleted.");

            entity.IsDeleted = true;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();

            _ticketSubTypeRepository.Update(entity);
            await _ticketSubTypeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetStatusAsync(Guid id, bool status, string actorUserId)
        {
            var entity = await _ticketSubTypeRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (entity.Status && !status && await _ticketSubTypeRepository.IsUsedByOpenTicketsAsync(id))
                throw new InvalidOperationException("Cannot deactivate: this Sub-Type is in use by open tickets.");

            entity.Status = status;
            entity.LastUpdatedDate = TimeHelper.GetIndianTime();
            entity.LastUpdatedByUserId = actorUserId;

            _ticketSubTypeRepository.Update(entity);
            await _ticketSubTypeRepository.SaveChangesAsync();
            return true;
        }

        private async Task ValidateCascadeAsync(Guid ticketTypeId, Guid departmentId, string? defaultUserId)
        {
            if (!await _ticketSubTypeRepository.IsTicketTypeActiveAsync(ticketTypeId))
                throw new InvalidOperationException("The selected Ticket Type must be active.");

            if (!await _ticketSubTypeRepository.DepartmentExistsAsync(departmentId))
                throw new InvalidOperationException("The selected Department does not exist.");

            if (!string.IsNullOrEmpty(defaultUserId) &&
                !await _ticketSubTypeRepository.IsActiveAgentInDepartmentAsync(defaultUserId, departmentId))
            {
                throw new InvalidOperationException(
                    "The selected Default Agent must be an active employee belonging to the chosen Department.");
            }
        }

        private static TicketSubTypeResponse MapToResponse(TicketSubType st)
        {
            return new TicketSubTypeResponse
            {
                TicketSubTypeId = st.TicketSubTypeId,
                Name = st.Name,
                Description = st.Description,
                TicketTypeId = st.TicketTypeId,
                TicketTypeName = st.TicketType?.Name,
                DepartmentId = st.DepartmentId,
                DepartmentName = st.Department?.Name,
                DefaultUserId = st.DefaultUserId,
                DefaultUserName = st.DefaultUser?.Name,
				RequiresAcceptance = st.RequiresAcceptance,
				AcceptanceDeadlineHours = st.AcceptanceDeadlineHours,
				ManualOnly = st.ManualOnly,
				Status = st.Status,
                CreatedDate = st.CreatedDate,
                LastUpdatedDate = st.LastUpdatedDate
            };
        }
    }
}
