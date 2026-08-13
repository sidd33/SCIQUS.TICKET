using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class TicketQueryParams
	{
		public string? Search { get; set; }

		public Guid? TicketTypeId { get; set; }
		public Guid? TicketSubTypeId { get; set; }
		public Guid? PriorityId { get; set; }
		public Guid? BusinessImpactId { get; set; }
		public Guid? StatusId { get; set; }

		public string? AccountId { get; set; }

		public string? AssignedToUserId { get; set; }

		public bool? IsInternal { get; set; }
		public bool? IsOpen { get; set; }

		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }

		public string? SortBy { get; set; }
		public bool SortDescending { get; set; } = false;

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;

		public Guid? DepartmentId { get; set; }

		// Ownership scoping (set server-side only, never trusted from client input)
		public string? OwnershipUserId { get; set; }
	}
}