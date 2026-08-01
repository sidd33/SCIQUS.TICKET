using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIQUSTICKETS.DATA.DomainModels
{
    public class AccountAddress
    {
        [Key]
        public Guid AccountAddressId { get; set; } = Guid.NewGuid();

        public string? Country { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }

        public string? MapUrl { get; set; }

        public string? AddressLine { get; set; }

        public bool PrimaryAddress { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public ICollection<AccountAddressType> AddressTypes { get; set; } = new List<AccountAddressType>();

        [Required]
        public string AccountId { get; set; } = string.Empty;

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
