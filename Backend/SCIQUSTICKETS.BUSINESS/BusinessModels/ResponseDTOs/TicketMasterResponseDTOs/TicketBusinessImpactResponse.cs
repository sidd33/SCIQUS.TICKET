namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs
{
    public class TicketBusinessImpactResponse
    {
        public Guid TicketBusinessTypeImpactId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
