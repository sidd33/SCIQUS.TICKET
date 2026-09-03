using System;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class UpdateDepartmentRequest
	{
		public string? Name { get; set; }

		public string? TicketAutoAssignMethod { get; set; }
		public double? W_Load { get; set; }
		public double? W_Severity { get; set; }
		public double? W_Recency { get; set; }
	}
}