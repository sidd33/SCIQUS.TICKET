using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class DepartmentResponse
	{
		public Guid DepartmentId { get; set; }
		public string Name { get; set; }
		public string? DepartmentHeadId { get; set; }
		public string? DepartmentHeadName { get; set; }
		public int EmployeeCount { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime LastModifiedDate { get; set; }
	}
}
