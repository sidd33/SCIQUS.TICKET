using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels.HolidayDATA
{
	public class Holiday
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public string Name { get; set; } = string.Empty;

		public DateTime Date { get; set; }

		// Hint only, for admin UI / a yearly-reseed job.
		// The actual Holiday row still exists per calendar year.
		public bool IsRecurringYearly { get; set; } = false;

		public string? Description { get; set; }

		public bool IsDeleted { get; set; } = false;

		public DateTime CreatedDate { get; set; }

		public ICollection<HolidayConfirmation> Confirmations { get; set; }
			= new List<HolidayConfirmation>();
	}
}