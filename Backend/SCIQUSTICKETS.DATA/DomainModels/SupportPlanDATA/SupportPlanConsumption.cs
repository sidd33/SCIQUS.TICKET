using System;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA
{
    public class SupportPlanConsumption
    {
        public Guid SupportPlanConsumptionId { get; set; } = Guid.NewGuid();
        
        public Guid AccountSupportPlanId { get; set; }
        public AccountSupportPlan AccountSupportPlan { get; set; } = null!;
        
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        
        public DateTime ConsumedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsRefunded { get; set; } = false;
        
        public bool IsOverage { get; set; } = false;
    }
}
