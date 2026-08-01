using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class EmployeeResponse
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string? RegisteredMobileNumber { get; set; }
		public string? SecondMobileNumber { get; set; }
		public string Email { get; set; }
		public string? EmployeeId { get; set; }
		public string? Designation { get; set; }
		public string? ReportsTo { get; set; }
		public string? ReportsToName { get; set; }
		public Guid DepartmentId { get; set; }
		public string? DepartmentName { get; set; }
		public Guid? GradeId { get; set; }
		public int? GradeLevel { get; set; }
		public string? ProfileImageUrl { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime LastUpdatedDate { get; set; }
	}
}
