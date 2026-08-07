namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class TicketDetailResponse : TicketResponse
	{
		public List<TicketCommentResponse> Comments { get; set; }
			= new List<TicketCommentResponse>();

		public List<TicketHistoryResponse> History { get; set; }
			= new List<TicketHistoryResponse>();

		public List<TicketAssignmentResponse> Assignments { get; set; }
			= new List<TicketAssignmentResponse>();

		public List<TicketAttachmentResponse> Attachments { get; set; }
			= new List<TicketAttachmentResponse>();
	}
}