// SCIQUSTICKETS.BUSINESS/BusinessModels/ResponseDTOs/TicketMasterResponseDTOs/FaqArticleResponse.cs
namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs
{
	public class FaqArticleResponse
	{
		public Guid FaqArticleId { get; set; }
		public Guid TicketTypeId { get; set; }
		public string? TicketTypeName { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Body { get; set; } = string.Empty;
		public bool Status { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime LastUpdatedDate { get; set; }
	}
}