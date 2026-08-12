using System;
using System.Collections.Generic;

namespace SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA
{
    public class AccountSupportPlan
    {
        public Guid AccountSupportPlanId { get; set; } = Guid.NewGuid();
        
        public string AccountId { get; set; } = null!;
        public Account Account { get; set; } = null!;
        
        public Guid SupportPlanId { get; set; }
        public SupportPlan SupportPlan { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // e.g., "Active", "Expired", "Cancelled"
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedDate { get; set; }
        public string? CreatedByUserId { get; set; }
        
        public ICollection<SupportPlanConsumption> Consumptions { get; set; } = new List<SupportPlanConsumption>();
    }
}
