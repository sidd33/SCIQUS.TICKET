using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketStateChangeHistory
	{
		[Key]
		public Guid TicketStateChangeHistoryId { get; set; } = Guid.NewGuid();

		public Guid TicketId { get; set; }
		[ForeignKey("TicketId")]
		public virtual Ticket Ticket { get; set; } = null!;

		public string ChangeType { get; set; } = null!; // "Priority", "Impact", "Status"
		public string? OldValue { get; set; }
		public string? NewValue { get; set; }
		public string Reason { get; set; } = null!;

		public string ChangedByUserId { get; set; } = null!;
		[ForeignKey("ChangedByUserId")]
		public virtual ApplicationUser ChangedByUser { get; set; } = null!;

		public DateTime CreatedDate { get; set; }
	}
}
