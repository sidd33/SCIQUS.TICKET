// SCIQUSTICKETS.BUSINESS/BusinessModels/RequestDTOs/TicketRequestDTOs/TicketReportQueryParams.cs
namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs
{
	public class TicketReportQueryParams
	{
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public Guid? DepartmentId { get; set; }
	}
}