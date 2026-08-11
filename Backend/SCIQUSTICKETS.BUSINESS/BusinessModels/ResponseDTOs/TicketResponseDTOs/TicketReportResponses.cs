// SCIQUSTICKETS.BUSINESS/BusinessModels/ResponseDTOs/TicketResponseDTOs/TicketReportResponses.cs
namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class DashboardSummaryResponse
	{
		public int OpenCount { get; set; }
		public int ClosedCount { get; set; }
		public int BreachedCount { get; set; }
		public int ClosedTodayCount { get; set; }
		public double AverageResolutionTimeInHours { get; set; }
	}

	public class GroupCountResponse
	{
		public string Key { get; set; } = string.Empty;
		public string Label { get; set; } = string.Empty;
		public int Count { get; set; }
	}

	public class SlaComplianceResponse
	{
		public int MetCount { get; set; }
		public int MissedCount { get; set; }
		public int NotApplicableCount { get; set; }
		public double MetPercentage { get; set; }
		public double AverageResolutionTimeInHours { get; set; }
		public double AverageOverdueHoursOnMisses { get; set; }
	}

	public class ReopenRateResponse
	{
		public int TotalTickets { get; set; }
		public int ReopenedTickets { get; set; }
		public double ReopenRatePercentage { get; set; }
	}

	public class AgentWorkloadResponse
	{
		public string AssignedToUserId { get; set; } = string.Empty;
		public string? AssignedToUserName { get; set; }
		public int OpenTicketCount { get; set; }
	}

	public class ChannelMixResponse
	{
		public string SourceType { get; set; } = string.Empty;
		public int Count { get; set; }
		public double Percentage { get; set; }
	}
}