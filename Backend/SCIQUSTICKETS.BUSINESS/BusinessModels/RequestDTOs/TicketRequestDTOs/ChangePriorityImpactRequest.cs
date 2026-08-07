using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class ChangePriorityImpactRequest
	{
		public Guid? PriorityId { get; set; }
		public Guid? BusinessImpactId { get; set; }

		[Required]
		public string Reason { get; set; } = null!;
	}
}
