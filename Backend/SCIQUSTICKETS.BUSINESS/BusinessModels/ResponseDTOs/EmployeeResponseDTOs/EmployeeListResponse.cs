using System;
using System.Collections.Generic;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class EmployeeListResponse
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string? Designation { get; set; }
		public Guid? DepartmentId { get; set; }
		public string? DepartmentName { get; set; }
		public string? ProfileImageUrl { get; set; }
		public List<string> Roles { get; set; } = new();
	}
}