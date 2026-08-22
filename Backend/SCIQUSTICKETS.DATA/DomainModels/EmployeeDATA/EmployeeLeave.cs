using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

public class EmployeeLeave
{
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required]
	public string EmployeeId { get; set; }

	[ForeignKey(nameof(EmployeeId))]
	public Employee Employee { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public string? LeaveType { get; set; }

	public string Status { get; set; } = "Approved";

	public bool IsDeleted { get; set; } = false;

	public DateTime CreatedDate { get; set; }
}