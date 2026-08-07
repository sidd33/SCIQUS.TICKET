namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs
{
    public class TicketSubTypeResponse
    {
        public Guid TicketSubTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid TicketTypeId { get; set; }
        public string? TicketTypeName { get; set; }

        public Guid DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public string? DefaultUserId { get; set; }
        public string? DefaultUserName { get; set; }

        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
