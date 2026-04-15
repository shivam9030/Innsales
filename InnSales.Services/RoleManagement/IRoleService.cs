using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace InnSales.Services
{
    public interface IRoleService
    {
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<IdentityResult> AssignRoleToUserAsync(string email, string roleName);
    }
}