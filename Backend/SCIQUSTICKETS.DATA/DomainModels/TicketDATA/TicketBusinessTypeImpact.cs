using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
    public class TicketBusinessTypeImpact
    {
        [Key]
        public Guid TicketBusinessTypeImpactId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public string? CreatedByUserId { get; set; }
        public string? LastUpdatedByUserId { get; set; }
    }
}
