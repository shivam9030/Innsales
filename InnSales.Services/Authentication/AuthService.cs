using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using InnSales.Domain.UserManagement.Entities;
using InnSales.Common.DTO.Auth;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace InnSales.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        //ObjectResult with status
        private ObjectResult Result(object value, int statusCode) =>
            new ObjectResult(value) { StatusCode = statusCode };

        // find user by email
        private async Task<ApplicationUser?> FindUserByEmailAsync(string email) =>
            string.IsNullOrEmpty(email) ? null : await _userManager.FindByEmailAsync(email);

        //find user by Id
        private async Task<ApplicationUser?> FindUserByIdAsync(string userId) =>
            string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);

        //Hash token
        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        //store hashed refresh token with expiry in AspNetUserTokens
        private async Task SetRefreshTokenAsync(ApplicationUser user, string token, int expiryDays)
        {
            var tokenData = new RefreshTokenData
            {
                Token = HashToken(token),
                Expiry = DateTime.UtcNow.AddDays(expiryDays)
            };

            var jsonValue = JsonSerializer.Serialize(tokenData);
            await _userManager.SetAuthenticationTokenAsync(user, "InnSales", "RefreshToken", jsonValue);
        }

        //verify refresh token
        private async Task<bool> VerifyRefreshTokenAsync(ApplicationUser user, string token)
        {
            var jsonValue = await _userManager.GetAuthenticationTokenAsync(user, "InnSales", "RefreshToken");
            if (jsonValue == null) return false;

            try
            {
                var tokenData = JsonSerializer.Deserialize<RefreshTokenData>(jsonValue);
                if (tokenData == null) return false;

                if (HashToken(token) != tokenData.Token) return false;
                if (tokenData.Expiry <= DateTime.UtcNow) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // generate access + refresh tokens
        private async Task<TokenResponseDto> GenerateTokensAsync(ApplicationUser user, IList<string> roles)
        {
            var accessToken = JwtTokenGenerator.GenerateToken(user, roles, _config);
            var refreshToken = JwtTokenGenerator.GenerateRefreshToken();
            var expiryDays = int.TryParse(_config["Jwt:RefreshTokenValidityInDays"], out var d) ? d : 7;

            await SetRefreshTokenAsync(user, refreshToken, expiryDays);

            var expiresInMinutes = int.TryParse(_config["Jwt:AccessTokenValidityInMinutes"], out var m) ? m : 60;

            return new TokenResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresInMinutes = expiresInMinutes,
                Roles = roles
            };
        }

        public async Task<IActionResult> RegisterAsync(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName,
                Department = model.Department,
                OfficeLocation = model.OfficeLocation,
                JoinedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Result("User registered successfully.", 200);

            return Result(result.Errors.Select(e => e.Description), 400);
        }

        public async Task<IActionResult> LoginAsync(LoginDto model)
        {
            var user = await FindUserByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Result("Invalid credentials.", 401);

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = await GenerateTokensAsync(user, roles);

            return Result(tokenResponse, 200);
        }

        public async Task<IActionResult> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var user = await FindUserByEmailAsync(email);

            if (string.IsNullOrEmpty(email))
                return Result("Email claim not found in token.", 400);
            if (user == null)
                return Result("User not found.", 404);

            var roles = await _userManager.GetRolesAsync(user);

            return Result(new
            {
                displayName = user.DisplayName,
                email = user.Email,
                roles
            }, 200);
        }

        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.RefreshToken))
                return Result("Token and RefreshToken are required.", 400);

            var principal = JwtTokenGenerator.GetPrincipalFromExpiredToken(model.Token, _config);
            if (principal == null)
                return Result("Invalid access token.", 400);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await FindUserByIdAsync(userId);

            if (string.IsNullOrEmpty(userId))
                return Result("User identifier missing in token.", 400);
            if (user == null)
                return Result("User not found.", 404);

            if (!await VerifyRefreshTokenAsync(user, model.RefreshToken))
                return Result("Invalid or expired refresh token.", 401);

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = await GenerateTokensAsync(user, roles);

            return Result(tokenResponse, 200);
        }

        // DTO for storing refresh token + expiry
        private class RefreshTokenData
        {
            public string Token { get; set; } = string.Empty;
            public DateTime Expiry { get; set; }
        }
    }
}
