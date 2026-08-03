using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface IAccountContactService
    {
        Task<List<AccountContactResponse>> GetContactsByAccountIdAsync(string accountId);
        Task<AccountContactResponse> AddContactAsync(string accountId, AccountContactResponse request);
        Task<bool> SetPrimaryContactAsync(string accountId, Guid contactId);
        Task<bool> SoftDeleteContactAsync(Guid contactId);
    }
}
