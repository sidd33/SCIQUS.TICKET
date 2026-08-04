using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.DomainModels
{
    public class Account
    {
        [Key]
        public string AccountId { get; set; } = Guid.NewGuid().ToString();

        [ForeignKey("AccountId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public string AutoGenerateAccountId { get; set; } = string.Empty;

        [Required]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        public string RegisteredMobileNumber { get; set; } = string.Empty;

        public string? SecondMobileNumber { get; set; }

        public string? Website { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        public string? AccountProfileImg { get; set; }

        public bool Status { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? IncorporationDate { get; set; }

        public DateTime? AccountSince { get; set; }

        public string? EmployeeCount { get; set; }

        public bool KeyAccount { get; set; } = false;

        public string? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual Employee? CreatedByUser { get; set; }

        public string? AccountManagerId { get; set; }
        [ForeignKey("AccountManagerId")]
        public virtual Employee? AccountManager { get; set; }

        public string? CustomerSuccessManagerId { get; set; }
        [ForeignKey("CustomerSuccessManagerId")]
        public virtual Employee? CustomerSuccessManager { get; set; }

        public Guid? AccountTypesId { get; set; }
        [ForeignKey("AccountTypesId")]
        public virtual AccountTypes? AccountTypes { get; set; }

        public Guid? IndustryTypeId { get; set; }
        [ForeignKey("IndustryTypeId")]
        public virtual IndustryTypes? IndustryTypes { get; set; }

        public Guid? RegionId { get; set; }
        [ForeignKey("RegionId")]
        public virtual Region? Region { get; set; }

        public Guid? CurrencyId { get; set; }
        [ForeignKey("CurrencyId")]
        public virtual Currency? Currency { get; set; }

        public string? DefaultCurrencySymbol { get; set; }

        public string? ConvertCurrencySymbol { get; set; }

        public string? ParentAccountId { get; set; }
        [ForeignKey("ParentAccountId")]
        public virtual Account? ParentAccount { get; set; }

        public string? ReferralAccountId { get; set; }
        [ForeignKey("ReferralAccountId")]
        public virtual Account? ReferralAccount { get; set; }

        public Guid? ReferralAccountContactsId { get; set; }
        [ForeignKey("ReferralAccountContactsId")]
        public virtual AccountContacts? ReferralAccountContacts { get; set; }

        public virtual ICollection<AccountContacts> Contacts { get; set; } = new List<AccountContacts>();

        public virtual ICollection<AccountAddress> Addresses { get; set; } = new List<AccountAddress>();
    }
}
