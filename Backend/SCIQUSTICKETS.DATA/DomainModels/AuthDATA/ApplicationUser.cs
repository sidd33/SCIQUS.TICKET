using Microsoft.AspNetCore.Identity;

namespace SCIQUSTICKETS.DATA.DomainModels.AuthDATA
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public bool Status { get; set; }
        public bool HasLoginAccess { get; set; } = false;

        public ICollection<SpecializedPolicy> SpecializedPolicies { get; set; } = new List<SpecializedPolicy>();
    }
}
