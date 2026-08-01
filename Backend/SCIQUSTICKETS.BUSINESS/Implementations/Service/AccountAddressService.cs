using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class AccountAddressService : IAccountAddressService
    {
        private readonly IAccountAddressRepository _addressRepository;

        public AccountAddressService(IAccountAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<List<AccountAddressResponse>> GetAddressesByAccountIdAsync(string accountId)
        {
            var addresses = await _addressRepository.GetByAccountIdAsync(accountId);
            return addresses.Select(MapToResponse).ToList();
        }

        public async Task<AccountAddressResponse> AddAddressAsync(string accountId, AccountAddressResponse request)
        {
            var address = new AccountAddress
            {
                AccountId = accountId,
                Country = request.Country,
                City = request.City,
                State = request.State,
                Pincode = request.Pincode,
                MapUrl = request.MapUrl,
                AddressLine = request.AddressLine,
                PrimaryAddress = request.PrimaryAddress
            };

            await _addressRepository.AddAsync(address);
            await _addressRepository.SaveChangesAsync();

            if (request.PrimaryAddress)
            {
                await _addressRepository.SetPrimaryAddressAsync(accountId, address.AccountAddressId);
            }

            return MapToResponse(address);
        }

        public async Task<bool> SetPrimaryAddressAsync(string accountId, Guid addressId)
        {
            return await _addressRepository.SetPrimaryAddressAsync(accountId, addressId);
        }

        public async Task<bool> SoftDeleteAddressAsync(Guid addressId)
        {
            return await _addressRepository.SoftDeleteAsync(addressId);
        }

        private static AccountAddressResponse MapToResponse(AccountAddress ad)
        {
            return new AccountAddressResponse
            {
                AccountAddressId = ad.AccountAddressId,
                Country = ad.Country,
                City = ad.City,
                State = ad.State,
                Pincode = ad.Pincode,
                MapUrl = ad.MapUrl,
                AddressLine = ad.AddressLine,
                PrimaryAddress = ad.PrimaryAddress,
                AccountId = ad.AccountId,
                AddressTypes = ad.AddressTypes?.Select(at => at.TypeName).ToList() ?? new List<string>()
            };
        }
    }
}
