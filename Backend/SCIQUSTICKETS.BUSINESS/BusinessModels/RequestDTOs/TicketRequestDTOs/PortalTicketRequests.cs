// SCIQUSTICKETS.BUSINESS/BusinessModels/RequestDTOs/TicketRequestDTOs/PortalTicketRequests.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class PortalCreateTicketRequest
	{
		[Required, MaxLength(200)]
		public string Title { get; set; } = null!;

		[Required]
		public string Description { get; set; } = null!;

		[Required]
		public Guid TicketTypeId { get; set; }

		[Required]
		public Guid TicketSubTypeId { get; set; }

		[Required]
		public Guid PriorityId { get; set; }

		[Required]
		public Guid BusinessImpactId { get; set; }

		public IFormFile? Attachment { get; set; }
	}

	public class PortalAddCommentRequest
	{
		[Required]
		public string Comment { get; set; } = null!;
	}
}