using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketComment
	{
		public Guid TicketCommentId { get; set; }


		public Guid TicketId { get; set; }

		public Ticket Ticket { get; set; } = null!;


		public string CommentText { get; set; } = null!;


		public string CommentedByUserId { get; set; } = null!;

		public ApplicationUser CommentedByUser { get; set; } = null!;


		public bool IsInternalNote { get; set; }

		public string? CommentedByAccountId { get; set; }


		public DateTime CreatedDate { get; set; }


		public bool IsDeleted { get; set; }
	}
}