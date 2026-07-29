namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.AccountRequestDTOs
{
    public class AccountQueryParams
    {
        public bool? IsDeleted { get; set; } = false;

        public string? Search { get; set; }

        public string SortBy { get; set; } = "CreatedDate";

        public bool SortDescending { get; set; } = true;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
