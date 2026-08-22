using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class CreateEmployeeWorkingHourRequest
	{
		[Required]
		public DayOfWeek DayOfWeek { get; set; }

		[Required]
		public TimeSpan StartTime { get; set; }

		[Required]
		public TimeSpan EndTime { get; set; }

		public bool IsWorkingDay { get; set; } = true;
	}
}
