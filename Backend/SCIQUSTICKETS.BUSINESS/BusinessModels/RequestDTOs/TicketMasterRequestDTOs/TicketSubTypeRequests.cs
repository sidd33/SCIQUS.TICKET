using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs
{
    public class CreateTicketSubTypeRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public Guid TicketTypeId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        // Optional: the agent auto-assigned when a ticket picks this sub-type
        public string? DefaultUserId { get; set; }
    }

    public class UpdateTicketSubTypeRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public Guid TicketTypeId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        public string? DefaultUserId { get; set; }

        public bool Status { get; set; }
    }
}
