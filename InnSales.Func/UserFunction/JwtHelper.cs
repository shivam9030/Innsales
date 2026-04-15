using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace InnSales.Func.UserFunction
{
    public static class JwtHelper
    {
        public static ClaimsPrincipal? Validate(string token, IConfiguration config)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = config["JWT_ISSUER"],

                ValidateAudience = true,
                ValidAudience = config["JWT_AUDIENCE"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["JWT_SECRET"])
                ),

                ValidateLifetime = false 
            };

            try
            {
                return new JwtSecurityTokenHandler()
                    .ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
