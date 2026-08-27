using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

public class EmployeeEmailNotificationPreference
{
	[Key]
	public Guid EmployeeEmailNotificationPreferenceId { get; set; }

	[Required]
	public string EmployeeId { get; set; }

	[ForeignKey(nameof(EmployeeId))]
	public Employee Employee { get; set; }

	public bool ReceiveAll { get; set; } = false;

	public bool Assignment { get; set; } = false;

	public bool Acceptance { get; set; } = false;

	public bool Rejection { get; set; } = false;

	public bool Expiry { get; set; } = false;

	public bool Reassignment { get; set; } = false;

	public bool StatusChange { get; set; } = false;

	public bool Closure { get; set; } = false;

	public bool Reopen { get; set; } = false;

	public DateTime CreatedDate { get; set; }

	public DateTime LastUpdatedDate { get; set; }
}