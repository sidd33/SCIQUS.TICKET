namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs
{
    public class AccountListResponse
    {
        public string AccountId { get; set; } = string.Empty;
        public string AutoGenerateAccountId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string RegisteredMobileNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AccountProfileImg { get; set; }
        public bool Status { get; set; }
        public bool KeyAccount { get; set; }
        public string? AccountTypeName { get; set; }
        public string? IndustryTypeName { get; set; }
        public string? RegionName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
