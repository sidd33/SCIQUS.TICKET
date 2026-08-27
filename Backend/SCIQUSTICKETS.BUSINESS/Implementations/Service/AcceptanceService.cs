// SCIQUSTICKETS.BUSINESS/Implementations/Service/AcceptanceService.cs
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Constants;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class AcceptanceService : IAcceptanceService
	{
		private readonly AppDbContext _context;
		private readonly IAssignmentEngine _assignmentEngine;
		private readonly IAcknowledgementService _acknowledgementService;
		private readonly IEmployeeEmailNotificationService _employeeEmailNotificationService;
		public AcceptanceService(
	AppDbContext context,
	IAssignmentEngine assignmentEngine,
	IAcknowledgementService acknowledgementService,
	IEmployeeEmailNotificationService employeeEmailNotificationService)
		{
			_context = context;
			_assignmentEngine = assignmentEngine;
			_acknowledgementService = acknowledgementService;
			_employeeEmailNotificationService = employeeEmailNotificationService;
		}

		public async Task StartAcceptanceCycleAsync(Ticket ticket, Employee employee, int attemptNumber)
		{
			await _context.TicketAcceptances.AddAsync(new TicketAcceptance
			{
				TicketId = ticket.TicketId,
				AssignedEmployeeId = employee.Id,
				Status = "Pending",
				AssignedAt = ticket.AcceptanceDeadlineAt!.Value.AddHours(-(ticket.AcceptanceDeadlineHours ?? 0)),
				AcceptanceDeadlineAt = ticket.AcceptanceDeadlineAt.Value,
				AttemptNumber = attemptNumber
			});
		}

		public async Task<bool> AcceptAsync(Guid ticketId, string employeeId)
		{
			var ticket = await _context.Tickets
				.FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);

			if (ticket == null) return false;

			if (!string.Equals(
					ticket.AcceptanceStatus,
					"Pending",
					StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException(
					"This ticket is not awaiting acceptance.");
			}

			if (ticket.AssignedToUserId != employeeId)
			{
				throw new InvalidOperationException(
					"Only the assigned agent can accept this ticket.");
			}

			var acceptanceRow = await _context.TicketAcceptances
				.Where(a =>
					a.TicketId == ticketId &&
					a.AssignedEmployeeId == employeeId &&
					a.Status == "Pending")
				.OrderByDescending(a => a.AttemptNumber)
				.FirstOrDefaultAsync();

			var now = TimeHelper.GetIndianTime();

			if (acceptanceRow != null)
			{
				acceptanceRow.Status = "Accepted";
				acceptanceRow.RespondedAt = now;
			}

			ticket.AcceptanceStatus = "Accepted";

			var inProgressStatus = await _context.TicketStatuses
	.FirstOrDefaultAsync(
		s => s.Name == "In Progress" && !s.IsDeleted)
	?? throw new InvalidOperationException(
		"The 'In Progress' status is not seeded.");

			ticket.StatusId = inProgressStatus.TicketStatusId;

			ticket.LastUpdatedDate = now;

			_context.TicketHistories.Add(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = "Ticket accepted by assigned agent",
				ChangedByUserId = employeeId,
				CreatedDate = now
			});

			await _context.SaveChangesAsync();

			await _acknowledgementService.HandleAsync(
				ticket.TicketId,
				"InProgress",
				employeeId);
			await _employeeEmailNotificationService.SendTicketNotificationAsync(
				ticket.TicketId,
				"Accepted",
				employeeId);
			return true;
		}

		public async Task<bool> RejectAsync(Guid ticketId, string employeeId, string reason)
		{
			if (string.IsNullOrWhiteSpace(reason))
				throw new InvalidOperationException("A rejection reason is required.");

			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);
			if (ticket == null) return false;

			if (!string.Equals(ticket.AcceptanceStatus, "Pending", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("This ticket is not awaiting acceptance.");

			if (ticket.AssignedToUserId != employeeId)
				throw new InvalidOperationException("Only the assigned agent can reject this ticket.");

			var now = TimeHelper.GetIndianTime();

			var acceptanceRow = await _context.TicketAcceptances
				.Where(a => a.TicketId == ticketId && a.AssignedEmployeeId == employeeId && a.Status == "Pending")
				.OrderByDescending(a => a.AttemptNumber)
				.FirstOrDefaultAsync();

			if (acceptanceRow != null)
			{
				acceptanceRow.Status = "Rejected";
				acceptanceRow.RespondedAt = now;
				acceptanceRow.RejectionReason = reason;
			}

			_context.TicketHistories.Add(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = $"Ticket rejected by agent. Reason: {reason}",
				ChangedByUserId = employeeId,
				CreatedDate = now
			});

			await _employeeEmailNotificationService.SendTicketNotificationAsync(
	ticket.TicketId,
	"Rejected",
	employeeId,
	reason);


			await RouteToFallbackOrQueueAsync(ticket, employeeId, now);

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task ProcessExpiredAsync()
		{
			var now = TimeHelper.GetIndianTime();

			var expiredTickets = await _context.Tickets
				.Where(t => t.AcceptanceStatus == "Pending"
					&& t.AcceptanceDeadlineAt != null
					&& t.AcceptanceDeadlineAt < now
					&& !t.IsDeleted)
				.ToListAsync();

			foreach (var ticket in expiredTickets)
			{
				var acceptanceRow = await _context.TicketAcceptances
					.Where(a => a.TicketId == ticket.TicketId && a.Status == "Pending")
					.OrderByDescending(a => a.AttemptNumber)
					.FirstOrDefaultAsync();

				if (acceptanceRow != null)
				{
					acceptanceRow.Status = "Expired";
					acceptanceRow.RespondedAt = now;
				}

				var expiredAgentId = ticket.AssignedToUserId;

				_context.TicketHistories.Add(new TicketHistory
				{
					TicketHistoryId = Guid.NewGuid(),
					TicketId = ticket.TicketId,
					ChangeDescription = "Acceptance expired (no response within deadline)",
					ChangedByUserId = SEED.SystemActorUserId,
					CreatedDate = now
				});

				if (!string.IsNullOrEmpty(expiredAgentId))
				{
					await RouteToFallbackOrQueueAsync(
						ticket,
						expiredAgentId,
						now);
				}
				else
				{
					ClearAcceptanceAndQueue(ticket);
				}
				// NOTE: Module 5 manager-escalation notification hook goes here.
			}

			if (expiredTickets.Count > 0)
				await _context.SaveChangesAsync();
		}

		/// <summary>
		/// Shared by Reject and Expire: excludes every agent who already rejected/expired on
		/// this ticket, tries the next fallback agent via the assignment engine, and stops
		/// rotating once MaxFallbackAttempts is reached (parks in queue + manager escalation).
		/// </summary>
		private async Task RouteToFallbackOrQueueAsync(Ticket ticket, string excludedAgentId, DateTime now)
		{
			var config = await _context.SlaConfigurations.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive);
			int maxAttempts = config?.MaxFallbackAttempts ?? 3;

			var alreadyExcluded = await _context.TicketAcceptances
				.Where(a => a.TicketId == ticket.TicketId && (a.Status == "Rejected" || a.Status == "Expired"))
				.Select(a => a.AssignedEmployeeId)
				.ToListAsync();

			var excludedSet = new HashSet<string>(alreadyExcluded) { excludedAgentId };

			if (ticket.CurrentFallbackAttempt >= maxAttempts)
			{
				ClearAcceptanceAndQueue(ticket);
				// NOTE: Module 5 manager-escalation notification hook goes here (fallback limit reached).
				return;
			}

			var nextAgent = await _assignmentEngine.ResolveAssigneeAsync(ticket, null, excludedSet);

			if (nextAgent == null)
			{
				ClearAcceptanceAndQueue(ticket);
				return;
			}

			ticket.AssignedToUserId = nextAgent.Id;
			ticket.CurrentFallbackAttempt += 1;
			ticket.AcceptanceStatus = "Pending";
			int deadlineHours = ticket.AcceptanceDeadlineHours ?? 2;
			ticket.AcceptanceDeadlineAt = now.AddHours(deadlineHours);
			ticket.LastUpdatedDate = now;

			_context.TicketAssignments.Add(new TicketAssignment
			{
				TicketAssignmentId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				AssignedToUserId = nextAgent.Id,
				AssignedByUserId = SEED.SystemActorUserId,
				AssignedDate = now,
				Status = "Assigned",
				IsAutoAssigned = true,
				Remarks = "Fallback re-route after reject/expire"
			});

			_context.TicketAcceptances.Add(new TicketAcceptance
			{
				TicketId = ticket.TicketId,
				AssignedEmployeeId = nextAgent.Id,
				Status = "Pending",
				AssignedAt = now,
				AcceptanceDeadlineAt = ticket.AcceptanceDeadlineAt.Value,
				AttemptNumber = ticket.CurrentFallbackAttempt
			});

			_context.TicketHistories.Add(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				ChangeDescription = $"Re-routed to fallback agent: {nextAgent.Name} (attempt {ticket.CurrentFallbackAttempt})",
				ChangedByUserId = SEED.SystemActorUserId,
				CreatedDate = now
			});

			await _employeeEmailNotificationService.SendTicketNotificationAsync(
				ticket.TicketId,
				"FallbackAssigned",
				SEED.SystemActorUserId,
				"The ticket has been reassigned to you after the previous acceptance attempt was rejected or expired.");
		}

		private static void ClearAcceptanceAndQueue(Ticket ticket)
		{
			ticket.AssignedToUserId = null;
			ticket.AcceptanceStatus = null;
			ticket.AcceptanceDeadlineAt = null;
		}
	}
}