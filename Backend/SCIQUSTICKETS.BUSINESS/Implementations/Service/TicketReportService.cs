// SCIQUSTICKETS.BUSINESS/Implementations/Service/TicketReportService.cs
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class TicketReportService : ITicketReportService
	{
		private readonly AppDbContext _context;

		public TicketReportService(AppDbContext context)
		{
			_context = context;
		}

		// All queries exclude soft-deleted tickets and apply the shared date-range/department filter.
		private IQueryable<Ticket> BaseQuery(TicketReportQueryParams query)
		{
			var q = _context.Tickets.AsNoTracking().Where(t => !t.IsDeleted);

			if (query.FromDate.HasValue) q = q.Where(t => t.CreatedDate >= query.FromDate.Value);
			if (query.ToDate.HasValue) q = q.Where(t => t.CreatedDate <= query.ToDate.Value);
			if (query.DepartmentId.HasValue) q = q.Where(t => t.DepartmentId == query.DepartmentId.Value);

			return q;
		}

		public async Task<DashboardSummaryResponse> GetSummaryAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);
			var now = TimeHelper.GetIndianTime();
			var todayStart = now.Date;

			var openCount = await q.CountAsync(t => t.IsOpen);
			var closedCount = await q.CountAsync(t => !t.IsOpen);
			var breachedCount = await q.CountAsync(t => t.IsOpen && t.IsSlaBreached == true);
			var closedToday = await q.CountAsync(t => t.ClosedDate != null && t.ClosedDate.Value >= todayStart);

			var resolutionTimes = await q
			.Where(t => t.ResolutionTimeInHours != null)
			.Select(t => t.ResolutionTimeInHours!.Value)
			.ToListAsync();

			var avgResolution = resolutionTimes.Count > 0
				? resolutionTimes.Average()
				: 0;

			return new DashboardSummaryResponse
			{
				OpenCount = openCount,
				ClosedCount = closedCount,
				BreachedCount = breachedCount,
				ClosedTodayCount = closedToday,
				AverageResolutionTimeInHours = Math.Round(avgResolution, 2)
			};
		}

		public async Task<IEnumerable<GroupCountResponse>> GetCountsByDepartmentAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);
			return await q
				.GroupBy(t => new { t.DepartmentId, t.Department.Name })
				.Select(g => new GroupCountResponse
				{
					Key = g.Key.DepartmentId.ToString(),
					Label = g.Key.Name,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.ToListAsync();
		}

		public async Task<IEnumerable<GroupCountResponse>> GetCountsByPriorityAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);
			return await q
				.GroupBy(t => new { t.PriorityId, t.Priority.Name })
				.Select(g => new GroupCountResponse
				{
					Key = g.Key.PriorityId.ToString(),
					Label = g.Key.Name,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.ToListAsync();
		}

		public async Task<IEnumerable<GroupCountResponse>> GetCountsByTypeAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);
			return await q
				.GroupBy(t => new { t.TicketTypeId, t.TicketType.Name })
				.Select(g => new GroupCountResponse
				{
					Key = g.Key.TicketTypeId.ToString(),
					Label = g.Key.Name,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.ToListAsync();
		}

		public async Task<IEnumerable<GroupCountResponse>> GetCountsByStatusAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);
			return await q
				.GroupBy(t => new { t.StatusId, t.Status.Name })
				.Select(g => new GroupCountResponse
				{
					Key = g.Key.StatusId.ToString(),
					Label = g.Key.Name,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.ToListAsync();
		}

		public async Task<SlaComplianceResponse> GetSlaComplianceAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);

			var metCount = await q.CountAsync(t => t.SlaMetStatus == "Met");
			var missedCount = await q.CountAsync(t => t.SlaMetStatus == "Missed");
			var naCount = await q.CountAsync(t => t.SlaMetStatus == "Not Applicable");

			var totalEvaluated = metCount + missedCount;
			var metPercentage = totalEvaluated > 0
				? Math.Round((double)metCount / totalEvaluated * 100, 2)
				: 0;

			var resolutionTimes = await q
			.Where(t => t.ResolutionTimeInHours != null)
			.Select(t => t.ResolutionTimeInHours!.Value)
			.ToListAsync();

			var avgResolution = resolutionTimes.Count > 0
				? resolutionTimes.Average()
				: 0;

			var overdueHours = await q
			.Where(t => t.SlaMetStatus == "Missed" && t.OverdueHours != null)
			.Select(t => t.OverdueHours!.Value)
			.ToListAsync();

			var avgOverdueOnMisses = overdueHours.Count > 0
				? overdueHours.Average()
				: 0;

			return new SlaComplianceResponse
			{
				MetCount = metCount,
				MissedCount = missedCount,
				NotApplicableCount = naCount,
				MetPercentage = metPercentage,
				AverageResolutionTimeInHours = Math.Round(avgResolution, 2),
				AverageOverdueHoursOnMisses = Math.Round(avgOverdueOnMisses, 2)
			};
		}

		public async Task<ReopenRateResponse> GetReopenRateAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);

			var total = await q.CountAsync();
			var reopened = await q.CountAsync(t => t.ReopenCount > 0);

			return new ReopenRateResponse
			{
				TotalTickets = total,
				ReopenedTickets = reopened,
				ReopenRatePercentage = total > 0 ? Math.Round((double)reopened / total * 100, 2) : 0
			};
		}

		public async Task<IEnumerable<AgentWorkloadResponse>> GetAgentWorkloadAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query).Where(t => t.IsOpen && t.AssignedToUserId != null);

			return await q
				.GroupBy(t => new { t.AssignedToUserId, t.AssignedToUser!.UserName })
				.Select(g => new AgentWorkloadResponse
				{
					AssignedToUserId = g.Key.AssignedToUserId!,
					AssignedToUserName = g.Key.UserName,
					OpenTicketCount = g.Count()
				})
				.OrderByDescending(x => x.OpenTicketCount)
				.ToListAsync();
		}

		public async Task<IEnumerable<ChannelMixResponse>> GetChannelMixAsync(TicketReportQueryParams query)
		{
			var q = BaseQuery(query);
			var total = await q.CountAsync();

			var grouped = await q
				.GroupBy(t => t.SourceType)
				.Select(g => new { SourceType = g.Key, Count = g.Count() })
				.ToListAsync();

			return grouped
				.Select(g => new ChannelMixResponse
				{
					SourceType = g.SourceType,
					Count = g.Count,
					Percentage = total > 0 ? Math.Round((double)g.Count / total * 100, 2) : 0
				})
				.OrderByDescending(x => x.Count)
				.ToList();
		}
	}
}