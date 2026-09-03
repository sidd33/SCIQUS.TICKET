// SCIQUSTICKETS.BUSINESS/Implementations/Service/SlaService.cs
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Constants;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class SlaService : ISlaService
	{
		private readonly AppDbContext _context;
		private readonly ITicketNotificationService _notificationService;
		private const string SystemActorId = "SYSTEM";

		public SlaService(AppDbContext context, ITicketNotificationService notificationService)
		{
			_context = context;
			_notificationService = notificationService;
		}

		public DateTime GetClockStart(Ticket ticket)
	=> ticket.EmailReceivedDate ?? ticket.CreatedDate;

		public (DateTime? SlaDueDate, string? SlaMetStatus) ComputeSlaDueDate(
			Ticket ticket,
			TicketPriority? priority)
		{
			if (priority == null || priority.SlaInHours <= 0)
				return (null, "Not Applicable");

			var clockStart = GetClockStart(ticket);

			var dueDate = clockStart.AddHours(priority.SlaInHours);

			return (dueDate, null);
		}

		public void FinalizeOnClose(Ticket ticket, string closureConfirmedBy)
		{
			var now = TimeHelper.GetIndianTime();
			var clockStart = GetClockStart(ticket);

			ticket.ClosedDate = now;
			ticket.ResolutionTimeInHours = (now - clockStart).TotalHours;
			ticket.ClosureConfirmedBy = closureConfirmedBy;

			if (ticket.SlaDueDate.HasValue)
			{
				ticket.SlaMetStatus = now <= ticket.SlaDueDate.Value ? "Met" : "Missed";
				ticket.OverdueHours = ticket.SlaMetStatus == "Missed"
					? (now - ticket.SlaDueDate.Value).TotalHours
					: null;
			}
		}

		public void ApplyReopen(Ticket ticket, TicketPriority? priority)
		{
			var now = TimeHelper.GetIndianTime();

			ticket.ReopenCount += 1;
			ticket.LastReopenedDate = now;
			ticket.IsOpen = true;
			ticket.ClosedDate = null;
			ticket.IsSlaBreached = false;
			ticket.ClosureConfirmedBy = null;
			ticket.SlaMetStatus = null;
			ticket.ResolutionTimeInHours = null;
			ticket.OverdueHours = null;

			if (priority != null && priority.SlaInHours > 0)
				ticket.SlaDueDate = now.AddHours(priority.SlaInHours);
			else
			{
				ticket.SlaDueDate = null;
				ticket.SlaMetStatus = "Not Applicable";
			}
		}

		public async Task ProcessBreachesAsync()
		{
			var now = TimeHelper.GetIndianTime();

			// 1. Process Breaches
			var breached = await _context.Tickets
				.Where(t => t.IsOpen && !t.IsDeleted
					&& t.SlaDueDate != null
					&& now > t.SlaDueDate
					&& (t.IsSlaBreached == null || t.IsSlaBreached == false))
				.ToListAsync();

			foreach (var ticket in breached)
			{
				ticket.IsSlaBreached = true;

				_context.TicketHistories.Add(new TicketHistory
				{
					TicketHistoryId = Guid.NewGuid(),
					TicketId = ticket.TicketId,
					ChangeDescription = "SLA breached",
					ChangedByUserId = SEED.SystemActorUserId,
					CreatedDate = now
				});
				await _notificationService.NotifySlaBreachedAsync(ticket.TicketId);
			}

			// 2. Process Warnings
			var upcomingBreaches = await _context.Tickets
				.Where(t => t.IsOpen && !t.IsDeleted
					&& t.SlaDueDate != null
					&& t.SlaDueDate > now
					&& t.SlaDueDate <= now.AddHours(4) // Check within 4 hours
					&& (t.IsSlaBreached == null || t.IsSlaBreached == false))
				.ToListAsync();

			foreach(var ticket in upcomingBreaches)
			{
				// Has it already been warned?
				bool alreadyWarned = await _context.TicketHistories
					.AnyAsync(th => th.TicketId == ticket.TicketId && th.ChangeDescription == "SLA Warning");
				
				if (alreadyWarned) continue;

				bool shouldWarn = false;
				if (!string.IsNullOrEmpty(ticket.AccountId))
				{
					var plan = await _context.AccountSupportPlans
						.Include(asp => asp.SupportPlan)
						.Where(asp => asp.AccountId == ticket.AccountId && asp.Status == "Active")
						.Select(asp => asp.SupportPlan)
						.FirstOrDefaultAsync();
						
					if (plan != null && (plan.EscalationLevel == "FastAlerts" || plan.EscalationLevel == "WarningAlerts" || plan.EscalationLevel == "Immediate"))
					{
						shouldWarn = true;
					}
				}

				if (shouldWarn)
				{
					_context.TicketHistories.Add(new TicketHistory
					{
						TicketHistoryId = Guid.NewGuid(),
						TicketId = ticket.TicketId,
						ChangeDescription = "SLA Warning",
						ChangedByUserId = SEED.SystemActorUserId,
						CreatedDate = now
					});
					
					await _notificationService.NotifySlaWarningAsync(ticket.TicketId);
				}
			}

			if (breached.Count > 0 || _context.ChangeTracker.HasChanges())
				await _context.SaveChangesAsync();
		}

		public async Task ProcessAutoClosuresAsync()
		{
			var config = await _context.SlaConfigurations.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive);
			if (config == null) return; // no active config → skip pass, no error (per doc)

			var now = TimeHelper.GetIndianTime();
			var pendingClosureStatus = await _context.TicketStatuses
				.FirstOrDefaultAsync(s => s.Name == "PendingClosure" && !s.IsDeleted);
			var closedStatus = await _context.TicketStatuses
				.FirstOrDefaultAsync(s => s.Name == "Closed" && !s.IsDeleted);

			if (pendingClosureStatus == null || closedStatus == null) return;

			var dueForAutoClose = await _context.Tickets
				.Where(t => t.StatusId == pendingClosureStatus.TicketStatusId
					&& !t.IsDeleted
					&& t.PendingClosureDate != null
					&& t.PendingClosureDate.Value.AddHours(config.AutoClosureHours) <= now)
				.ToListAsync();

			foreach (var ticket in dueForAutoClose)
			{
				var oldStatusId = ticket.StatusId;
				ticket.StatusId = closedStatus.TicketStatusId;
				ticket.IsOpen = false;
				ticket.LastUpdatedDate = now;

				FinalizeOnClose(ticket, "Auto");

				_context.TicketHistories.Add(new TicketHistory
				{
					TicketHistoryId = Guid.NewGuid(),
					TicketId = ticket.TicketId,
					OldStatusId = oldStatusId,
					NewStatusId = closedStatus.TicketStatusId,
					ChangeDescription = "Auto-closed (no customer response)",
					ChangedByUserId = SEED.SystemActorUserId,
					CreatedDate = now
				});
				await _notificationService.NotifyClosedAsync(ticket.TicketId, SystemActorId);
			}

			if (dueForAutoClose.Count > 0)
				await _context.SaveChangesAsync();
		}
	}
}