using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
            => await _context.Users.FindAsync(userId);

        public async Task<bool> UserExistsAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);


        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
            => await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && !rt.IsUsed);

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            tokens.ForEach(t => t.IsRevoked = true);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByRoleAsync(string roleId)
            => await _context.RolePolicies
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Policy)
                .Select(rp => rp.Policy)
                .ToListAsync();

        public async Task<IEnumerable<Policy>> GetSpecializedPoliciesByUserAsync(string userId)
            => await _context.SpecializedPolicies
                .Where(sp => sp.UserId == userId)
                .Include(sp => sp.Policy)
                .Select(sp => sp.Policy)
                .ToListAsync();
    }
}
