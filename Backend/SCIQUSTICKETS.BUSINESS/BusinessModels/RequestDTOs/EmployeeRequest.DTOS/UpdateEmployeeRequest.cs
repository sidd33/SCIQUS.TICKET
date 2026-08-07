using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class UpdateEmployeeRequest
	{
		public string? Name { get; set; }
		public string? RegisteredMobileNumber { get; set; }
		public string? SecondMobileNumber { get; set; }
		public string? Designation { get; set; }
		public string? ReportsTo { get; set; }
		public Guid? DepartmentId { get; set; }
		public Guid? GradeId { get; set; }
		public string? ProfileImageUrl { get; set; }
		// Email intentionally excluded — decide separately if email changes need a verification flow
	}
}
