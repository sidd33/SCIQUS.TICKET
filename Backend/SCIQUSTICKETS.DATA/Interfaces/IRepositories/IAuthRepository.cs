using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
    public interface IAuthRepository
    {
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<bool> UserExistsAsync(string email);

        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserTokensAsync(string userId);

        Task<IEnumerable<Policy>> GetPoliciesByRoleAsync(string roleId);
        Task<IEnumerable<Policy>> GetSpecializedPoliciesByUserAsync(string userId);
    }
}
