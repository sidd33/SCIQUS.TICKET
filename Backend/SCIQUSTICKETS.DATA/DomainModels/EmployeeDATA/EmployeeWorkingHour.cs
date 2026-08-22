using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

public class EmployeeWorkingHour
{
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required]
	public string EmployeeId { get; set; }

	[ForeignKey(nameof(EmployeeId))]
	public Employee Employee { get; set; }

	public DayOfWeek DayOfWeek { get; set; }

	public TimeSpan StartTime { get; set; }

	public TimeSpan EndTime { get; set; }

	public bool IsWorkingDay { get; set; } = true;
}
