
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
namespace InnSales.Services
{   
public class PaymentTokenService : IPaymentTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
       private readonly string _secret;

    public PaymentTokenService(IConfiguration config)
    {
        _issuer  = config["Payments:PaymentTokenIssuer"];
        _audience= config["Payments:PaymentTokenAudience"];
        _secret  = config["Payments:PaymentTokenSecret"];
    }

    
public string Issue(Guid orderId, decimal amount, string currency, string userId, int expMinutes = 10)
{
    var claims = new[]
    {
        new Claim("orderId", orderId.ToString()),
        new Claim("amount", amount.ToString("F2")),
        new Claim("currency", currency),
        new Claim("userId", userId)
    };

    var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var now   = DateTime.UtcNow;

    var token = new JwtSecurityToken(_issuer, _audience, claims, now, now.AddMinutes(expMinutes), creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}


public IDictionary<string, string> Validate(string token)
{
    var handler = new JwtSecurityTokenHandler();
    var parameters = new TokenValidationParameters
    {
               ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
        ValidateIssuer   = true,
        ValidIssuer      = _issuer,
        ValidateAudience = true,
        ValidAudience    = _audience,
        ValidateLifetime = true,
        ClockSkew        = TimeSpan.FromSeconds(30)
    };

    handler.ValidateToken(token, parameters, out var validatedToken);
    var jwt = (JwtSecurityToken)validatedToken;

    return jwt.Claims.ToDictionary(c => c.Type, c => c.Value);
}
}
}