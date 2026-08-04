namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs
{
    public class AccountContactResponse
    {
        public Guid AccountContactsId { get; set; }
        public string? PersonName { get; set; }
        public string? Designation { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? AlternateMobileNumber { get; set; }
        public string? Department { get; set; }
        public string? Branch { get; set; }
        public bool PrimaryContact { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime? DOB { get; set; }
        public string? Address { get; set; }
        public DateTime? AssociatedSince { get; set; }
        public string AccountId { get; set; } = string.Empty;
    }
}
