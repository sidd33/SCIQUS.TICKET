using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.HolidayDATA
{
	public class HolidayConfirmation
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public Guid HolidayId { get; set; }

		[ForeignKey(nameof(HolidayId))]
		public Holiday Holiday { get; set; }

		[Required]
		public string EmployeeId { get; set; }

		[ForeignKey(nameof(EmployeeId))]
		public Employee Employee { get; set; }

		// "Pending" | "Available" | "Unavailable"
		// Pending == treated as Unavailable by AssignmentEngine until employee confirms.
		public string Status { get; set; } = "Pending";

		public DateTime? RespondedDate { get; set; }

		public bool IsDeleted { get; set; } = false;

		public DateTime CreatedDate { get; set; }
	}
}