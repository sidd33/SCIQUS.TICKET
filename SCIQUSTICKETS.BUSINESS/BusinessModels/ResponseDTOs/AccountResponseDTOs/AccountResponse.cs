namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs
{
    public class AccountResponse
    {
        public string AccountId { get; set; } = string.Empty;
        public string AutoGenerateAccountId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string RegisteredMobileNumber { get; set; } = string.Empty;
        public string? SecondMobileNumber { get; set; }
        public string? Website { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? AccountProfileImg { get; set; }
        public bool Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public DateTime? IncorporationDate { get; set; }
        public DateTime? AccountSince { get; set; }
        public string? EmployeeCount { get; set; }
        public bool KeyAccount { get; set; }

        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }

        public string? AccountManagerId { get; set; }
        public string? AccountManagerName { get; set; }

        public string? CustomerSuccessManagerId { get; set; }
        public string? CustomerSuccessManagerName { get; set; }

        public Guid? AccountTypesId { get; set; }
        public string? AccountTypeName { get; set; }

        public Guid? IndustryTypeId { get; set; }
        public string? IndustryTypeName { get; set; }

        public Guid? RegionId { get; set; }
        public string? RegionName { get; set; }

        public Guid? CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? DefaultCurrencySymbol { get; set; }
        public string? ConvertCurrencySymbol { get; set; }

        public string? ParentAccountId { get; set; }
        public string? ParentAccountName { get; set; }

        public string? ReferralAccountId { get; set; }
        public string? ReferralAccountName { get; set; }

        public Guid? ReferralAccountContactsId { get; set; }
        public string? ReferralAccountContactName { get; set; }

        public List<AccountContactResponse> Contacts { get; set; } = new List<AccountContactResponse>();
        public List<AccountAddressResponse> Addresses { get; set; } = new List<AccountAddressResponse>();
    }
}
