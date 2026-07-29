using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class EmployeeListResponse
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string? Designation { get; set; }
		public string? DepartmentName { get; set; }
		public string? ProfileImageUrl { get; set; }
	}
}