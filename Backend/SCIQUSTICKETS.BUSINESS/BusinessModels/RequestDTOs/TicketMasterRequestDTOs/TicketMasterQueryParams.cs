namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs
{
    /// <summary>
    /// Shared list/search params for the Module 1 master lists
    /// (Ticket Types, Priorities, Business Impacts). TicketSubType has its
    /// own params (TicketSubTypeQueryParams) since it needs extra filters.
    /// </summary>
    public class TicketMasterQueryParams
    {
        // Default: only Status == true && IsDeleted == false.
        // Set true to include inactive (but never deleted) rows.
        public bool IncludeInactive { get; set; } = false;

        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
