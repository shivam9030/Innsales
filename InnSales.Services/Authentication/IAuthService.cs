
using Microsoft.AspNetCore.Mvc;
using InnSales.Common.DTO.Auth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InnSales.Services
{
    public interface IAuthService
    {
        Task<IActionResult> RegisterAsync(RegisterDto model);
        Task<IActionResult> LoginAsync(LoginDto model);
        Task<IActionResult> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<IActionResult> RefreshTokenAsync(RefreshTokenDto model);
    }
}
