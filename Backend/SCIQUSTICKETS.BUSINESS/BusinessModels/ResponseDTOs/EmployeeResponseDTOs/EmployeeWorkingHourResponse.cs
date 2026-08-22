using System;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class EmployeeWorkingHourResponse
	{
		public Guid Id { get; set; }

		public string EmployeeId { get; set; } = string.Empty;

		public DayOfWeek DayOfWeek { get; set; }

		public TimeSpan StartTime { get; set; }

		public TimeSpan EndTime { get; set; }

		public bool IsWorkingDay { get; set; }
	}
}
