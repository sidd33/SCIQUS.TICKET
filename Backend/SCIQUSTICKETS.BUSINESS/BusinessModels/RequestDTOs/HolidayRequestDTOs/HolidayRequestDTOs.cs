using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.HolidayRequestDTOs
{
	public class CreateHolidayRequest
	{
		[Required]
		public string Name { get; set; } = string.Empty;

		[Required]
		public DateTime Date { get; set; }

		public bool IsRecurringYearly { get; set; } = false;

		public string? Description { get; set; }
	}

	public class UpdateHolidayRequest
	{
		[Required]
		public string Name { get; set; } = string.Empty;

		[Required]
		public DateTime Date { get; set; }

		public bool IsRecurringYearly { get; set; } = false;

		public string? Description { get; set; }
	}

	public class ConfirmHolidayRequest
	{
		// true = "I will be available / working this holiday"
		// false = "I will be unavailable"
		[Required]
		public bool IsAvailable { get; set; }
	}
}