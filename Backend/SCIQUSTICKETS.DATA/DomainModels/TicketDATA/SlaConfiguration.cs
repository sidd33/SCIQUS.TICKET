using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class SlaConfiguration
	{
		[Key]
		public Guid SlaConfigurationId { get; set; } = Guid.NewGuid();

		public string DefaultAutoAssignMethod { get; set; } = "LoadBalanced"; // "RoundRobin", "LoadBalanced", "Auto_assignment_custom"

		public double DefaultW_Load { get; set; } = 1.0;
		public double DefaultW_Severity { get; set; } = 0.5;
		public double DefaultW_Recency { get; set; } = 0.1;
		public int DefaultRecencyCapHours { get; set; } = 48;
		public int DefaultMaxConsecutiveAssignments { get; set; } = 3;
		public int DefaultMaxConcurrentOpenTickets { get; set; } = 10;
		public int MaxFallbackAttempts { get; set; } = 3;

		public int AutoClosureHours { get; set; } = 48;
		public bool AllowEmployeeReopen { get; set; } = true;
		public int ReopenGraceDays { get; set; } = 7;

		public bool IsActive { get; set; } = true;
		public DateTime CreatedDate { get; set; }
		public DateTime LastUpdatedDate { get; set; }

		public bool RequiresAcceptanceGlobalDefault { get; set; } = false;

		public int DefaultAcceptanceDeadlineHours { get; set; } = 2;
	}
}
