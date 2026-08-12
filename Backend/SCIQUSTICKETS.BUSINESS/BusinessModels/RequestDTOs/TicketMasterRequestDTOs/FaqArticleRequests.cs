// SCIQUSTICKETS.BUSINESS/BusinessModels/RequestDTOs/TicketMasterRequestDTOs/FaqArticleRequests.cs
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs
{
	public class CreateFaqArticleRequest
	{
		[Required]
		public Guid TicketTypeId { get; set; }

		[Required, MaxLength(200)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Body { get; set; } = string.Empty;
	}

	public class UpdateFaqArticleRequest
	{
		[Required, MaxLength(200)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Body { get; set; } = string.Empty;

		public bool Status { get; set; }
	}
}