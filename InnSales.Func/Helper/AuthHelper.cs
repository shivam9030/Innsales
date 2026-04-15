
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Helper
{
    public class AuthHelper : IAuthHelper
    {
        private readonly IConfiguration _config;

        public AuthHelper(IConfiguration config)
        {
            _config = config;
        }

        public bool IsAuthenticated(HttpRequestData req)
        {
            return ValidateToken(req, out _);
        }

        public bool HasRole(HttpRequestData req, string role)
        {
            if (!ValidateToken(req, out var principal) || principal is null) return false;

            var roles = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role 
                         || c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) 
                         || c.Type.Equals("roles", StringComparison.OrdinalIgnoreCase))
                .SelectMany(c => c.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(r => r.Trim());

            return roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        }

        public HttpResponseData Unauthorized(HttpRequestData req)
        {
            var res = req.CreateResponse(HttpStatusCode.Unauthorized);
            res.Headers.Add("WWW-Authenticate", "Bearer");
            return res;
        }

        public HttpResponseData Forbidden(HttpRequestData req)
        {
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }

        private bool ValidateToken(HttpRequestData req, out ClaimsPrincipal? principal)
        {
            principal = null;

            if (!req.Headers.TryGetValues("Authorization", out var headers)) return false;
            var raw = headers.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;

            var token = raw.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrWhiteSpace(token)) return false;

            // Read from your local.settings.json Values
            var issuer = _config["JWT_ISSUER"];
            var audience = _config["JWT_AUDIENCE"];
            var key = _config["JWT_SECRET"];
            if (string.IsNullOrWhiteSpace(key)) return false;

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = audience,
                ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateLifetime = true
            };

            try
            {
                principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
                return principal is not null;
            }
            catch
            {
                return false;
            }
        }
    }
}
