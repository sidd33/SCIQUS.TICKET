using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Enums;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.COMMON.Helpers;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class TicketTimelineService : ITicketTimelineService
    {
        private readonly AppDbContext _context;

        public TicketTimelineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task WriteHistoryAsync(Guid ticketId, TicketChangeType changeType, string? oldValue, string? newValue, string description, string actorId, bool isAccountActor = false)
        {
            var history = new TicketStateChangeHistory
            {
                TicketId = ticketId,
                ChangeType = changeType.ToString(),
                OldValue = oldValue,
                NewValue = newValue,
                Reason = description,
				ChangedByUserId = isAccountActor ? null : actorId,
				ChangedByAccountId = isAccountActor ? actorId : null,
				CreatedDate = TimeHelper.GetIndianTime()
            };

            _context.TicketStateChangeHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TimelineEventResponse>> GetTimelineAsync(Guid ticketId, bool excludeInternal)
        {
            var timeline = new List<TimelineEventResponse>();

            // 1. TicketStateChangeHistory
            var stateChanges = await _context.TicketStateChangeHistories
                .Include(h => h.ChangedByUser)
                .Where(h => h.TicketId == ticketId)
                .ToListAsync();

            timeline.AddRange(stateChanges.Select(h => new TimelineEventResponse
            {
                EventId = h.TicketStateChangeHistoryId,
                ChangeType = h.ChangeType,
                ActorId = h.ChangedByUserId,
                ActorName = h.ChangedByUser?.UserName ?? h.ChangedByUserId,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                Description = h.Reason,
                Timestamp = h.CreatedDate,
                IsInternalNote = false
            }));

            // 2. TicketHistory (legacy status changes)
            var ticketHistories = await _context.TicketHistories
                .Include(h => h.ChangedByUser)
                .Where(h => h.TicketId == ticketId)
                .ToListAsync();

            timeline.AddRange(ticketHistories.Select(h => new TimelineEventResponse
            {
                EventId = h.TicketHistoryId,
                ChangeType = TicketChangeType.StatusChanged.ToString(),
                ActorId = h.ChangedByUserId,
                ActorName = h.ChangedByUser?.UserName ?? h.ChangedByUserId,
                OldValue = h.OldStatusId?.ToString(),
                NewValue = h.NewStatusId?.ToString(),
                Description = h.ChangeDescription,
                Timestamp = h.CreatedDate,
                IsInternalNote = false
            }));

            // 3. TicketAssignments
            var assignments = await _context.TicketAssignments
                .Include(a => a.AssignedByUser)
                .Include(a => a.AssignedToUser)
                .Where(a => a.TicketId == ticketId && !a.IsDeleted)
                .ToListAsync();

            timeline.AddRange(assignments.Select(a => new TimelineEventResponse
            {
                EventId = a.TicketAssignmentId,
                ChangeType = a.Status == "Transferred" ? TicketChangeType.Transferred.ToString() : TicketChangeType.Assigned.ToString(),
                ActorId = a.AssignedByUserId,
                ActorName = a.AssignedByUser?.UserName ?? a.AssignedByUserId,
                OldValue = null,
                NewValue = a.AssignedToUser?.UserName ?? a.AssignedToUserId,
                Description = string.IsNullOrWhiteSpace(a.Remarks) ? $"Assigned to {a.AssignedToUser?.UserName ?? a.AssignedToUserId}" : a.Remarks,
                Timestamp = a.AssignedDate,
                IsInternalNote = false
            }));

            // 4. TicketComments
            var commentsQuery = _context.TicketComments
                .Include(c => c.CommentedByUser)
                .Where(c => c.TicketId == ticketId && !c.IsDeleted);

            if (excludeInternal)
            {
                commentsQuery = commentsQuery.Where(c => !c.IsInternalNote);
            }

            var comments = await commentsQuery.ToListAsync();

            timeline.AddRange(comments.Select(c => new TimelineEventResponse
            {
                EventId = c.TicketCommentId,
                ChangeType = TicketChangeType.Commented.ToString(),
                ActorId = c.CommentedByUserId,
                ActorName = c.CommentedByUser?.UserName ?? c.CommentedByUserId,
                OldValue = null,
                NewValue = c.CommentText,
                Description = c.IsInternalNote ? "Added an internal note" : "Added a comment",
                Timestamp = c.CreatedDate,
                IsInternalNote = c.IsInternalNote
            }));

            return timeline.OrderByDescending(t => t.Timestamp).ToList();
        }
    }
}
