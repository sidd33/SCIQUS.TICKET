using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class CreateTicketRequest
	{
		[Required]
		[MaxLength(200)]
		public string Title { get; set; } = null!;


		[Required]
		public string Description { get; set; } = null!;


		// Customer ticket
		public string? AccountId { get; set; }


		// Internal employee ticket
		public string? RaisedByEmployeeId { get; set; }


		public bool IsInternal { get; set; }


		// Portal / Email / WhatsApp / Internal
		public string SourceType { get; set; } = "Portal";


		[Required]
		public Guid TicketTypeId { get; set; }


		[Required]
		public Guid TicketSubTypeId { get; set; }


		[Required]
		public Guid PriorityId { get; set; }


		[Required]
		public Guid BusinessImpactId { get; set; }
	}
}