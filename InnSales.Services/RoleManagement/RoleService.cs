using Microsoft.AspNetCore.Identity;
using InnSales.Domain.UserManagement.Entities;
using System;
using System.Threading.Tasks;

namespace InnSales.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty.");

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AssignRoleToUserAsync(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new Exception("Role does not exist.");

            return await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}