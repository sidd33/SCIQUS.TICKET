namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AuthResponseDTOs
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool Status { get; set; }
        public bool HasLoginAccess { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
