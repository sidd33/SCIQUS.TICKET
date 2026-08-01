namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs
{
    public class AccountAddressResponse
    {
        public Guid AccountAddressId { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? MapUrl { get; set; }
        public string? AddressLine { get; set; }
        public bool PrimaryAddress { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public List<string> AddressTypes { get; set; } = new List<string>();
    }
}
