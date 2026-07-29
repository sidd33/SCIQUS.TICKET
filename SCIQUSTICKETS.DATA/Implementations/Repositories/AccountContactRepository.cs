using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class AccountContactRepository : GenericRepository<AccountContacts>, IAccountContactRepository
    {
        private readonly AppDbContext _context;

        public AccountContactRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<AccountContacts>> GetByAccountIdAsync(string accountId)
        {
            return await _context.AccountContacts
                .Where(c => c.AccountId == accountId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> SetPrimaryContactAsync(string accountId, Guid contactId)
        {
            var contacts = await _context.AccountContacts
                .Where(c => c.AccountId == accountId && !c.IsDeleted)
                .ToListAsync();

            if (!contacts.Any(c => c.AccountContactsId == contactId)) return false;

            foreach (var contact in contacts)
            {
                contact.PrimaryContact = (contact.AccountContactsId == contactId);
                contact.LastUpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(Guid contactId)
        {
            var contact = await _context.AccountContacts.FindAsync(contactId);
            if (contact == null) return false;

            contact.IsDeleted = true;
            contact.LastUpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
