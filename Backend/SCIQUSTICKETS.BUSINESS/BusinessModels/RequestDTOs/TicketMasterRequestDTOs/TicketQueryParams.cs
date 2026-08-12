using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class TicketQueryParams
	{
		// Search
		public string? Search { get; set; }


		// Filters
		public Guid? TicketTypeId { get; set; }

		public Guid? TicketSubTypeId { get; set; }

		public Guid? PriorityId { get; set; }

		public Guid? BusinessImpactId { get; set; }

		public Guid? StatusId { get; set; }


		// CRM
		public string? AccountId { get; set; }


		// Assignment
		public string? AssignedToUserId { get; set; }


		// Internal / External
		public bool? IsInternal { get; set; }


		// Open / Closed
		public bool? IsOpen { get; set; }


		// Date filters
		public DateTime? FromDate { get; set; }

		public DateTime? ToDate { get; set; }


		// Sorting
		public string? SortBy { get; set; }

		public bool SortDescending { get; set; } = false;


		// Paging
		public int Page { get; set; } = 1;

		public int PageSize { get; set; } = 20;

		
		public Guid? DepartmentId { get; set; }
	}
}