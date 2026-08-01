using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels.AuthDATA
{
    public class Policy
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string PolicyGroup { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Key { get; set; }

        public ICollection<RolePolicy> RolePolicies { get; set; } = new List<RolePolicy>();
        public ICollection<SpecializedPolicy> SpecializedPolicies { get; set; } = new List<SpecializedPolicy>();
    }
}
