using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface ITicketSubTypeRepository : IGenericRepository<TicketSubType>
    {
        Task<(IEnumerable<TicketSubType> Items, int TotalCount)> GetAllPagedAsync(
            bool includeInactive,
            string? search,
            Guid? ticketTypeId,
            Guid? departmentId,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize);

        Task<TicketSubType?> GetByIdWithDetailsAsync(Guid ticketSubTypeId);

        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

        // TODO(Module 2): wire once the Ticket table exists.
        Task<bool> IsUsedByOpenTicketsAsync(Guid ticketSubTypeId);

        // Cascade validation helpers (Module 1 "how it must work" rules)
        Task<bool> IsTicketTypeActiveAsync(Guid ticketTypeId);
        Task<bool> DepartmentExistsAsync(Guid departmentId);

        /// <summary>
        /// True if the given employee is active (has login access, not deleted)
        /// AND belongs to the given department.
        /// </summary>
        Task<bool> IsActiveAgentInDepartmentAsync(string employeeId, Guid departmentId);
    }
}
