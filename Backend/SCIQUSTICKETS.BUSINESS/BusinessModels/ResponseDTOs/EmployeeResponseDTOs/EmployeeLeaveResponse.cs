using System;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class EmployeeLeaveResponse
	{
		public Guid Id { get; set; }

		public string EmployeeId { get; set; } = string.Empty;

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public string? LeaveType { get; set; }

		public string Status { get; set; } = string.Empty;

		public bool IsDeleted { get; set; }

		public DateTime CreatedDate { get; set; }
	}
}

