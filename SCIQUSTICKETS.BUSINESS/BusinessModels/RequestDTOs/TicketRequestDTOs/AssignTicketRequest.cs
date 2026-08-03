using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class AssignTicketRequest
	{
		public string AssignedToUserId { get; set; } = null!;

		public string? Remarks { get; set; }
	}
}

