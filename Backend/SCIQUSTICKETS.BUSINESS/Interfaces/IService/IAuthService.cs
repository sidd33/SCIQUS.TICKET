using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.AuthRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AuthResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshRequestDto request);
        Task RevokeTokenAsync(string refreshToken);
        Task<UserResponseDto> GetCurrentUserAsync(string userId);
    }
}
