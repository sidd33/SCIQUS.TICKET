using System;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.SupportPlanDTOs
{
    public class CreateSupportPlanRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int TicketQuota { get; set; }
        public string PeriodType { get; set; } = "Monthly";
        public int? ValidityDays { get; set; }
        public bool BlockWhenExhausted { get; set; } = true;
    }

    public class UpdateSupportPlanRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int TicketQuota { get; set; }
        public string PeriodType { get; set; } = "Monthly";
        public int? ValidityDays { get; set; }
        public bool BlockWhenExhausted { get; set; } = true;
        public bool Status { get; set; } = true;
    }

    public class SupportPlanResponse
    {
        public Guid SupportPlanId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int TicketQuota { get; set; }
        public string PeriodType { get; set; } = "Monthly";
        public int? ValidityDays { get; set; }
        public bool BlockWhenExhausted { get; set; }
        public bool Status { get; set; }
    }

    public class AssignPlanRequest
    {
        public string AccountId { get; set; } = null!;
        public Guid SupportPlanId { get; set; }
        // The service will automatically set StartDate to UTC now, and EndDate based on ValidityDays or a default (e.g. 1 year if monthly/yearly without validity days).
    }

    public class AccountSupportPlanResponse
    {
        public Guid AccountSupportPlanId { get; set; }
        public string AccountId { get; set; } = null!;
        public Guid SupportPlanId { get; set; }
        public string PlanName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        
        public int TicketQuota { get; set; }
        public int ConsumedQuota { get; set; }
        public int RemainingQuota { get; set; }
    }
}
