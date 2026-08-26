using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class CreateEmployeeLeaveRequest
	{
		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		public string? LeaveType { get; set; }
	}
}
