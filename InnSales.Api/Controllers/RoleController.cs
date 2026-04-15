using Microsoft.AspNetCore.Mvc;
using InnSales.Services;
using InnSales.Common.DTO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace InnSales.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var result = await _roleService.CreateRoleAsync(roleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Role created successfully.");
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto dto)
        {
            var result = await _roleService.AssignRoleToUserAsync(dto.Email, dto.RoleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Role assigned successfully.");
        }
    }
}