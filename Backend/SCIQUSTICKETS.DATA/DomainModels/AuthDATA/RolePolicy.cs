using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SCIQUSTICKETS.DATA.DomainModels.AuthDATA
{
    /// <summary>
    /// Join table between a Role and a Policy.
    /// Composite PK: (RoleId, PolicyId)
    /// </summary>
    public class RolePolicy
    {
        [Required]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        public Guid PolicyId { get; set; }

        // Navigation
        [JsonIgnore]
        [ForeignKey("RoleId")]
        public UserRole Role { get; set; } = null!;

        [JsonIgnore]
        [ForeignKey("PolicyId")]
        public Policy Policy { get; set; } = null!;
    }
}
