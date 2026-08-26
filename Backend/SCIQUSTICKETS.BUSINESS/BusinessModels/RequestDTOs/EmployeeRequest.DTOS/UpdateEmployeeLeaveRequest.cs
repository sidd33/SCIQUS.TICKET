using System;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class UpdateEmployeeLeaveRequest
	{
		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public string? LeaveType { get; set; }

		public string? Status { get; set; }
	}
}
