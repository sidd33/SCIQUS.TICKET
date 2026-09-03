using System;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class DepartmentResponse
	{
		public Guid DepartmentId { get; set; }

		public string Name { get; set; } = string.Empty;

		public Guid? DepartmentHeadId { get; set; }

		public string? DepartmentHeadName { get; set; }

		public int EmployeeCount { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime LastModifiedDate { get; set; }

		public string? TicketAutoAssignMethod { get; set; }

		public double? W_Load { get; set; }

		public double? W_Severity { get; set; }

		public double? W_Recency { get; set; }
	}
}