using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.AccountRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface IAccountService
    {
        Task<PagedResponse<AccountListResponse>> GetAllAccountsAsync(AccountQueryParams queryParams);
        Task<AccountResponse?> GetAccountByIdAsync(string accountId);
        Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
        Task<AccountResponse?> UpdateAccountAsync(string accountId, UpdateAccountRequest request);
        Task<bool> SoftDeleteAccountAsync(string accountId);
    }
}
