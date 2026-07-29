using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.AccountRequestDTOs
{
    public class CreateAccountRequest
    {
        [Required]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        public string RegisteredMobileNumber { get; set; } = string.Empty;

        public string? SecondMobileNumber { get; set; }

        public string? Website { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? AccountProfileImg { get; set; }

        public DateTime? IncorporationDate { get; set; }

        public DateTime? AccountSince { get; set; }

        public string? EmployeeCount { get; set; }

        public bool KeyAccount { get; set; } = false;

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [Required]
        public string AccountManagerId { get; set; } = string.Empty;

        public string? CustomerSuccessManagerId { get; set; }

        public Guid? AccountTypesId { get; set; }

        public Guid? IndustryTypeId { get; set; }

        public Guid? RegionId { get; set; }

        public Guid? CurrencyId { get; set; }

        public string? DefaultCurrencySymbol { get; set; }

        public string? ConvertCurrencySymbol { get; set; }

        public string? ParentAccountId { get; set; }

        public string? ReferralAccountId { get; set; }

        public Guid? ReferralAccountContactsId { get; set; }
    }
}
