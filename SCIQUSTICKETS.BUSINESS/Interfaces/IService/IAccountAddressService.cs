using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface IAccountAddressService
    {
        Task<List<AccountAddressResponse>> GetAddressesByAccountIdAsync(string accountId);
        Task<AccountAddressResponse> AddAddressAsync(string accountId, AccountAddressResponse request);
        Task<bool> SetPrimaryAddressAsync(string accountId, Guid addressId);
        Task<bool> SoftDeleteAddressAsync(Guid addressId);
    }
}
