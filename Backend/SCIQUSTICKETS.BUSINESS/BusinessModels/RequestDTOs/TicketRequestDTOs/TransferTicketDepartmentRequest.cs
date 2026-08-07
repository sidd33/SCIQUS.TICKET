using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class TransferTicketDepartmentRequest
	{
		[Required]
		public Guid DepartmentId { get; set; }

		public string? TargetAssigneeUserId { get; set; }

		[Required]
		public string Comment { get; set; } = null!;
	}
}
