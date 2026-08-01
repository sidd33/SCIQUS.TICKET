using Microsoft.AspNetCore.Identity;

namespace SCIQUSTICKETS.DATA.DomainModels.AuthDATA
{
    public class UserRole : IdentityRole
    {
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<RolePolicy> RolePolicies { get; set; } = new List<RolePolicy>();
    }
}
