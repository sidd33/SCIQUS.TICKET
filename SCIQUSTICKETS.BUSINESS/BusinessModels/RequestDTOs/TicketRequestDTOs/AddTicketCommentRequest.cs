using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class AddTicketCommentRequest
	{
		[Required]
		public string Comment { get; set; } = null!;
	}
}