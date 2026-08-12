using System;
using System.Collections.Generic;

namespace SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA
{
    public class SupportPlan
    {
        public Guid SupportPlanId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        
        public int TicketQuota { get; set; }
        
        // e.g., "OneTime", "Monthly", "Yearly"
        public string PeriodType { get; set; } = "Monthly";
        
        public int? ValidityDays { get; set; }
        
        public bool BlockWhenExhausted { get; set; } = true;
        
        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedDate { get; set; }
        public string? CreatedByUserId { get; set; }
        
        public ICollection<AccountSupportPlan> AccountSupportPlans { get; set; } = new List<AccountSupportPlan>();
    }
}
