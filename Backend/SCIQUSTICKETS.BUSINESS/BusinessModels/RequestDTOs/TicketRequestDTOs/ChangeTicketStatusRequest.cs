using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class ChangeTicketStatusRequest
	{
		[Required]
		public Guid StatusId { get; set; }


		public string? Comment { get; set; }
	}
}