namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs
{
    public class TicketSubTypeQueryParams : TicketMasterQueryParams
    {
        public Guid? TicketTypeId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
