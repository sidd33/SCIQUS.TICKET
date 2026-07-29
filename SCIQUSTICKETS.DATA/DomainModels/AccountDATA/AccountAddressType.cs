using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIQUSTICKETS.DATA.DomainModels
{
    public class AccountAddressType
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string TypeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid AccountAddressId { get; set; }

        [ForeignKey("AccountAddressId")]
        public virtual AccountAddress AccountAddress { get; set; } = null!;
    }
}
