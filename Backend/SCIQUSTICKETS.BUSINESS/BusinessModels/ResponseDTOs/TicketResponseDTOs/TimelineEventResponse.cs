using System;
using SCIQUSTICKETS.COMMON.Enums;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
    public class TimelineEventResponse
    {
        public Guid EventId { get; set; }
        public string ChangeType { get; set; } = null!;
        public string ActorId { get; set; } = null!;
        public string ActorName { get; set; } = null!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public bool IsInternalNote { get; set; }
    }
}
