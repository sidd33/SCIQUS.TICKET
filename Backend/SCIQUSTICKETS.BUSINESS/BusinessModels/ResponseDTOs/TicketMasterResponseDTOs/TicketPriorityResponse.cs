namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs
{
    public class TicketPriorityResponse
    {
        public Guid TicketPriorityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int SlaInHours { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
