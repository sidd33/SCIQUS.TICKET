using System;
using System.Collections.Generic;
using System.Text;
namespace SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams
{
	public class EmployeeQueryParams
	{
		// Filtering
		public Guid? DepartmentId { get; set; }
		public Guid? GradeId { get; set; }
		public string? ReportsTo { get; set; }
		public bool? IsDeleted { get; set; } = false;
		public string? Search { get; set; } // matches Name/Email/EmployeeId

		// Sorting
		public string? SortBy { get; set; } = "Name"; // Name, CreatedDate, Designation
		public bool SortDescending { get; set; } = false;

		// Pagination
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
	}
}
