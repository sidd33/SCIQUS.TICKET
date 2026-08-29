using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA
{
    public class AccountDedicatedEmployee
    {
        [Key]
        public Guid AccountDedicatedEmployeeId { get; set; } = Guid.NewGuid();

        [Required]
        public string AccountId { get; set; } = null!;

        [Required]
        public string EmployeeUserId { get; set; } = null!;

        // "Primary", "Backup", "AllocatedGroup"
        public string Role { get; set; } = "AllocatedGroup";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
