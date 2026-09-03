using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class DepartmentListResponse
	{
		public Guid DepartmentId { get; set; }
		public string Name { get; set; }
		public int EmployeeCount { get; set; }

		public string? TicketAutoAssignMethod { get; set; }
		public double? W_Load { get; set; }
		public double? W_Severity { get; set; }
		public double? W_Recency { get; set; }
	}
}
