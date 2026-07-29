using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.AuthRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.AuthResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<UserRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<UserRole> roleManager,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // ── Register ───────────────────────────────────────────────────

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (await _authRepository.UserExistsAsync(request.Email))
                throw new InvalidOperationException("A user with this email already exists.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Status = true,
                HasLoginAccess = true,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Assign role if provided and exists
            if (!string.IsNullOrWhiteSpace(request.Role) && await _roleManager.RoleExistsAsync(request.Role))
                await _userManager.AddToRoleAsync(user, request.Role);

            var roles = await _userManager.GetRolesAsync(user);
            var (accessToken, expiresAt) = GenerateAccessToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            await _authRepository.SaveRefreshTokenAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays())
            });

            return BuildAuthResponse(user, roles, accessToken, expiresAt, refreshToken);
        }

        // ── Login ──────────────────────────────────────────────────────

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.Status || !user.HasLoginAccess)
                throw new UnauthorizedAccessException("Your account is inactive. Contact an administrator.");

            // Revoke old tokens on new login
            await _authRepository.RevokeAllUserTokensAsync(user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            var (accessToken, expiresAt) = GenerateAccessToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            await _authRepository.SaveRefreshTokenAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays())
            });

            return BuildAuthResponse(user, roles, accessToken, expiresAt, refreshToken);
        }

        // ── Refresh Token ──────────────────────────────────────────────

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshRequestDto request)
        {
            var storedToken = await _authRepository.GetRefreshTokenAsync(request.RefreshToken)
                ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");

            // Mark old token as used
            storedToken.IsUsed = true;
            await _authRepository.RevokeRefreshTokenAsync(storedToken.Token);

            var user = storedToken.User;
            var roles = await _userManager.GetRolesAsync(user);
            var (accessToken, expiresAt) = GenerateAccessToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();

            await _authRepository.SaveRefreshTokenAsync(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays())
            });

            return BuildAuthResponse(user, roles, accessToken, expiresAt, newRefreshToken);
        }

        // ── Revoke ─────────────────────────────────────────────────────

        public async Task RevokeTokenAsync(string refreshToken)
            => await _authRepository.RevokeRefreshTokenAsync(refreshToken);

        // ── Get Current User ───────────────────────────────────────────

        public async Task<UserResponseDto> GetCurrentUserAsync(string userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);
            return MapToUserResponse(user, roles);
        }

        // ── Private Helpers ────────────────────────────────────────────

        private (string token, DateTime expiresAt) GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var expiresAt = DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private int GetRefreshTokenExpiryDays()
            => int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

        private static AuthResponseDto BuildAuthResponse(
            ApplicationUser user,
            IList<string> roles,
            string accessToken,
            DateTime expiresAt,
            string refreshToken)
        {
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = expiresAt,
                User = MapToUserResponse(user, roles)
            };
        }

        // Manual mapping — no AutoMapper
        private static UserResponseDto MapToUserResponse(ApplicationUser user, IList<string> roles)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Status = user.Status,
                HasLoginAccess = user.HasLoginAccess,
                Roles = roles
            };
        }
    }
}
