using System;
using System.ComponentModel.DataAnnotations;
using SCIQUSTICKETS.COMMON.Enums;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class CustomerNotificationPreference
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public EmailNotificationCategory Category { get; set; }

		public bool IsEnabled { get; set; } = false;

		public DateTime LastUpdatedDate { get; set; }
	}
}