using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams
{
	public class DepartmentQueryParams
	{
		public bool? IsDeleted { get; set; } = false;
		public string? Search { get; set; } // matches Name

		public string? SortBy { get; set; } = "Name";
		public bool SortDescending { get; set; } = false;

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
	}
}