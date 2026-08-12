using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SCIQUSTICKETS.COMMON.Enums;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface ITicketTimelineService
    {
        Task WriteHistoryAsync(Guid ticketId, TicketChangeType changeType, string? oldValue, string? newValue, string description, string actorId);
        Task<List<TimelineEventResponse>> GetTimelineAsync(Guid ticketId, bool excludeInternal);
    }
}
