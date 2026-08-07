using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class TicketAttachmentResponse
	{
		public Guid TicketAttachmentId { get; set; }

		public Guid TicketId { get; set; }


		public string FileName { get; set; } = null!;

		public string FilePath { get; set; } = null!;

		public string? FileType { get; set; }

		public long FileSize { get; set; }


		public string UploadedByUserId { get; set; } = null!;

		public string? UploadedByUserName { get; set; }


		public DateTime UploadedDate { get; set; }
	}
}
