using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
    public class TicketSubType
    {
        [Key]
        public Guid TicketSubTypeId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Foreign Key to TicketType
        public Guid TicketTypeId { get; set; }
        [ForeignKey("TicketTypeId")]
        public virtual TicketType TicketType { get; set; } = null!;

        // Foreign Key to Department
        public Guid DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        // Foreign Key to Employee (default auto-assigned agent) - optional
        public string? DefaultUserId { get; set; }
        [ForeignKey("DefaultUserId")]
        public virtual Employee? DefaultUser { get; set; }

        public bool? RequiresAcceptance { get; set; }
        public int? AcceptanceDeadlineHours { get; set; }
        public bool ManualOnly { get; set; } = false;

        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public string? CreatedByUserId { get; set; }
        public string? LastUpdatedByUserId { get; set; }
    }
}
