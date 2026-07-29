using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.AuthRequestDTOs
{
    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
