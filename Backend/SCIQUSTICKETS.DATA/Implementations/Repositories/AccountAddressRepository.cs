using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class AccountAddressRepository : GenericRepository<AccountAddress>, IAccountAddressRepository
    {
        private readonly AppDbContext _context;

        public AccountAddressRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<AccountAddress>> GetByAccountIdAsync(string accountId)
        {
            return await _context.AccountAddresses
                .Include(a => a.AddressTypes)
                .Where(a => a.AccountId == accountId && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> SetPrimaryAddressAsync(string accountId, Guid addressId)
        {
            var addresses = await _context.AccountAddresses
                .Where(a => a.AccountId == accountId && !a.IsDeleted)
                .ToListAsync();

            if (!addresses.Any(a => a.AccountAddressId == addressId)) return false;

            foreach (var address in addresses)
            {
                address.PrimaryAddress = (address.AccountAddressId == addressId);
                address.LastUpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(Guid addressId)
        {
            var address = await _context.AccountAddresses.FindAsync(addressId);
            if (address == null) return false;

            address.IsDeleted = true;
            address.LastUpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
