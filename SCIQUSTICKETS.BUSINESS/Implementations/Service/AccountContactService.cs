using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AccountResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class AccountContactService : IAccountContactService
    {
        private readonly IAccountContactRepository _contactRepository;

        public AccountContactService(IAccountContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<List<AccountContactResponse>> GetContactsByAccountIdAsync(string accountId)
        {
            var contacts = await _contactRepository.GetByAccountIdAsync(accountId);
            return contacts.Select(MapToResponse).ToList();
        }

        public async Task<AccountContactResponse> AddContactAsync(string accountId, AccountContactResponse request)
        {
            var contact = new AccountContacts
            {
                AccountId = accountId,
                PersonName = request.PersonName,
                Designation = request.Designation,
                Email = request.Email,
                MobileNumber = request.MobileNumber,
                AlternateMobileNumber = request.AlternateMobileNumber,
                Department = request.Department,
                Branch = request.Branch,
                PrimaryContact = request.PrimaryContact,
                ProfileImage = request.ProfileImage,
                DOB = request.DOB,
                Address = request.Address,
                AssociatedSince = request.AssociatedSince
            };

            await _contactRepository.AddAsync(contact);
            await _contactRepository.SaveChangesAsync();

            if (request.PrimaryContact)
            {
                await _contactRepository.SetPrimaryContactAsync(accountId, contact.AccountContactsId);
            }

            return MapToResponse(contact);
        }

        public async Task<bool> SetPrimaryContactAsync(string accountId, Guid contactId)
        {
            return await _contactRepository.SetPrimaryContactAsync(accountId, contactId);
        }

        public async Task<bool> SoftDeleteContactAsync(Guid contactId)
        {
            return await _contactRepository.SoftDeleteAsync(contactId);
        }

        private static AccountContactResponse MapToResponse(AccountContacts c)
        {
            return new AccountContactResponse
            {
                AccountContactsId = c.AccountContactsId,
                PersonName = c.PersonName,
                Designation = c.Designation,
                Email = c.Email,
                MobileNumber = c.MobileNumber,
                AlternateMobileNumber = c.AlternateMobileNumber,
                Department = c.Department,
                Branch = c.Branch,
                PrimaryContact = c.PrimaryContact,
                ProfileImage = c.ProfileImage,
                DOB = c.DOB,
                Address = c.Address,
                AssociatedSince = c.AssociatedSince,
                AccountId = c.AccountId
            };
        }
    }
}
