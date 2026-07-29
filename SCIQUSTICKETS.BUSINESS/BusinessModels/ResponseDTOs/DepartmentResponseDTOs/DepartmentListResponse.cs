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
	}
}
