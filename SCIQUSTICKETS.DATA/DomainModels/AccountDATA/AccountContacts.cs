using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIQUSTICKETS.DATA.DomainModels
{
    public class AccountContacts
    {
        [Key]
        public Guid AccountContactsId { get; set; } = Guid.NewGuid();

        public string? PersonName { get; set; }

        public string? Designation { get; set; }

        public string? Email { get; set; }

        public string? MobileNumber { get; set; }

        public string? AlternateMobileNumber { get; set; }

        public string? Department { get; set; }

        public string? Branch { get; set; }

        public bool PrimaryContact { get; set; } = false;

        public string? ProfileImage { get; set; }

        public DateTime? DOB { get; set; }

        public string? Address { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? AssociatedSince { get; set; }

        [Required]
        public string AccountId { get; set; } = string.Empty;

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
