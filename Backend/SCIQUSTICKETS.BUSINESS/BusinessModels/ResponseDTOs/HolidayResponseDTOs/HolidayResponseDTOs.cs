using System;
using System.Collections.Generic;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.HolidayResponseDTOs
{
	public class HolidayResponse
	{
		public Guid Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public DateTime Date { get; set; }

		public bool IsRecurringYearly { get; set; }

		public string? Description { get; set; }

		public bool IsDeleted { get; set; }

		public DateTime CreatedDate { get; set; }
	}

	public class HolidayConfirmationResponse
	{
		public Guid Id { get; set; }

		public Guid HolidayId { get; set; }

		public string HolidayName { get; set; } = string.Empty;

		public DateTime HolidayDate { get; set; }

		public string EmployeeId { get; set; } = string.Empty;

		public string Status { get; set; } = string.Empty;

		public DateTime? RespondedDate { get; set; }
	}
}