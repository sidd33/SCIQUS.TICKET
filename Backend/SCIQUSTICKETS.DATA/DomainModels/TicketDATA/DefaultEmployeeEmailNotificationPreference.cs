using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class DefaultEmployeeEmailNotificationPreference
	{
		[Key]
		public int Id { get; set; } // always 1 — single row

		public bool ReceiveAll { get; set; } = false;
		public bool Assignment { get; set; } = false;
		public bool Acceptance { get; set; } = false;
		public bool Rejection { get; set; } = false;
		public bool Expiry { get; set; } = false;
		public bool Reassignment { get; set; } = false;
		public bool StatusChange { get; set; } = false;
		public bool Closure { get; set; } = false;
		public bool Reopen { get; set; } = false;

		public DateTime LastUpdatedDate { get; set; }
	}
}