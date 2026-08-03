using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<Account> Items, int TotalCount)> GetAllPagedAsync(
            bool? isDeleted,
            string? search,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize)
        {
            var query = _context.Accounts
                .Include(a => a.AccountTypes)
                .Include(a => a.IndustryTypes)
                .Include(a => a.Region)
                .Include(a => a.Currency)
                .AsQueryable();

            if (isDeleted.HasValue)
            {
                query = query.Where(a => a.IsDeleted == isDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a =>
                    a.AccountName.ToLower().Contains(search) ||
                    a.AutoGenerateAccountId.ToLower().Contains(search) ||
                    a.Email.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Account?> GetWithDetailsByIdAsync(string accountId)
        {
            return await _context.Accounts
                .Include(a => a.AccountTypes)
                .Include(a => a.IndustryTypes)
                .Include(a => a.Region)
                .Include(a => a.Currency)
                .Include(a => a.Contacts)
                .Include(a => a.Addresses)
                    .ThenInclude(ad => ad.AddressTypes)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
        }

        public async Task<bool> SoftDeleteAsync(string accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;

            account.IsDeleted = true;
            account.LastUpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateNextAccountIdAsync()
        {
            int count = await _context.Accounts.CountAsync();
            int nextNumber = count + 1;
            return $"ACC-{DateTime.UtcNow.Year}-{nextNumber:D4}";
        }
    }
}
