// SCIQUSTICKETS.DATA/DomainModels/TicketDATA/FaqArticle.cs
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class FaqArticle
	{
		[Key]
		public Guid FaqArticleId { get; set; } = Guid.NewGuid();

		public Guid TicketTypeId { get; set; }
		public TicketType TicketType { get; set; } = null!;

		[Required, MaxLength(200)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Body { get; set; } = string.Empty;

		public bool Status { get; set; } = true;
		public bool IsDeleted { get; set; } = false;

		public DateTime CreatedDate { get; set; }
		public DateTime LastUpdatedDate { get; set; }

		public string? CreatedByUserId { get; set; }
	}
}