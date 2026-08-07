using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketAttachment
	{
		public Guid TicketAttachmentId { get; set; }


		public Guid TicketId { get; set; }

		public Ticket Ticket { get; set; } = null!;


		public string FileName { get; set; } = null!;


		public string FilePath { get; set; } = null!;


		public string? ContentType { get; set; }


		public long FileSize { get; set; }


		public string UploadedByUserId { get; set; } = null!;

		public ApplicationUser UploadedByUser { get; set; } = null!;


		public DateTime UploadedDate { get; set; }


		public bool IsDeleted { get; set; }
	}
}