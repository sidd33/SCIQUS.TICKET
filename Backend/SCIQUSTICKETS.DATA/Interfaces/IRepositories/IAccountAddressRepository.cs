using SCIQUSTICKETS.DATA.DomainModels;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface IAccountAddressRepository : IGenericRepository<AccountAddress>
    {
        Task<List<AccountAddress>> GetByAccountIdAsync(string accountId);
        Task<bool> SetPrimaryAddressAsync(string accountId, Guid addressId);
        Task<bool> SoftDeleteAsync(Guid addressId);
    }
}
