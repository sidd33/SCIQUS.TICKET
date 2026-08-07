using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class UpdateTicketRequest
	{
		[Required]
		[MaxLength(200)]
		public string Title { get; set; } = null!;


		[Required]
		public string Description { get; set; } = null!;


		public Guid TicketTypeId { get; set; }

		public Guid TicketSubTypeId { get; set; }

		public Guid PriorityId { get; set; }

		public Guid BusinessImpactId { get; set; }
	}
}