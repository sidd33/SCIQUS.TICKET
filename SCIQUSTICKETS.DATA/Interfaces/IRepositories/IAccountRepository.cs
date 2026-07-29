using SCIQUSTICKETS.DATA.DomainModels;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<(List<Account> Items, int TotalCount)> GetAllPagedAsync(
            bool? isDeleted,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize);

        Task<Account?> GetWithDetailsByIdAsync(string accountId);

        Task<bool> SoftDeleteAsync(string accountId);

        Task<string> GenerateNextAccountIdAsync();
    }
}
