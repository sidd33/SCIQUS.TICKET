using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SCIQUSTICKETS.DATA.DomainModels.AuthDATA
{
    /// <summary>
    /// Grants a specific Policy directly to a User (user-level override, bypasses role).
    /// Composite PK: (UserId, PolicyId)
    /// </summary>
    public class SpecializedPolicy
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation
        [JsonIgnore]
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [JsonIgnore]
        [ForeignKey("PolicyId")]
        public Policy Policy { get; set; } = null!;
    }
}
