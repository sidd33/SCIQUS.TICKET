using SCIQUSTICKETS.DATA.DomainModels;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface IAccountContactRepository : IGenericRepository<AccountContacts>
    {
        Task<List<AccountContacts>> GetByAccountIdAsync(string accountId);
        Task<bool> SetPrimaryContactAsync(string accountId, Guid contactId);
        Task<bool> SoftDeleteAsync(Guid contactId);
    }
}
