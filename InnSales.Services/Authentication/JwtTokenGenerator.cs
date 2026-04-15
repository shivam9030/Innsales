using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InnSales.Domain.UserManagement.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InnSales.Services
{
    public static class JwtTokenGenerator
    {
        public static string GenerateToken(ApplicationUser user, IList<string> roles, IConfiguration config)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("DisplayName", user.DisplayName ?? string.Empty),
                new Claim("Department", user.Department ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // UPDATED KEYS HERE
            var keyBytes = Encoding.UTF8.GetBytes(config["JWT_SECRET"]);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = config["JWT_ISSUER"];
            var audience = config["JWT_AUDIENCE"];
            var validityMinutes = int.TryParse(config["JWT_ACCESS_TOKEN_VALIDITY_MINUTES"], out var minutes)
                ? minutes : 60;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(validityMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            RandomNumberGenerator.Fill(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, IConfiguration config)
        {
            //UPDATED KEYS HERE ALSO
            var parameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = config["JWT_AUDIENCE"],
                ValidateIssuer = true,
                ValidIssuer = config["JWT_ISSUER"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["JWT_SECRET"])
                ),
                ValidateLifetime = false // allow expired token
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameters, out var securityToken);

                if (securityToken is JwtSecurityToken jwt &&
                    jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
