using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace FekraHubAPI.Controllers.AuthorizationController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =DefaultRole.Admin)]
    public class AuthorizationUsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthorizationUsersController(UserManager<ApplicationUser> userManager , RoleManager<IdentityRole> roleManager) 
        {
            _userManager = userManager;
            _roleManager = roleManager;


        }


        [HttpPost("[action]")]
        public async Task<IActionResult> AssignPermissionToRole([FromQuery][Required] string RoleName, [FromQuery][Required] string PermissionName)
        {
            var role = await _roleManager.FindByNameAsync(RoleName);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            bool claimExists = roleClaims.Any(c => c.Type == PermissionName && c.Value == PermissionName);

            var isTrue = PermissionsEnum.CheckPermissionExist(PermissionName);
            // check permission is exist 
            if (PermissionsEnum.CheckPermissionExist(PermissionName) && !claimExists)
            {
                await _roleManager.AddClaimAsync(role, new Claim(PermissionName , PermissionName));
                return Ok("Permission Assigned Successfully");
            }
            return BadRequest("Error In Assign");
        } 
        [HttpPost("[action]")]
        public async Task<IActionResult> RemovePermissionToRole([FromQuery][Required] string RoleName, [FromQuery][Required] string PermissionName)
        {
            var role = await _roleManager.FindByNameAsync(RoleName);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            // check permission is exist 
            if (PermissionsEnum.CheckPermissionExist(PermissionName))
            {
                await _roleManager.RemoveClaimAsync(role, new Claim(PermissionName, PermissionName));
                return Ok("Permission Removed Successfully");
            }
            return BadRequest("Error In Assign");
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> RolePermissions()
        {
            var rolesWithPermissions = new Dictionary<string, List<string>>();

            var roles = await _roleManager.Roles.ToListAsync();
            foreach (var role in roles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                if (!rolesWithPermissions.ContainsKey(role.Name))
                {
                    rolesWithPermissions[role.Name] = new List<string>();
                }
                foreach (var roleClaim in roleClaims)
                {
                    rolesWithPermissions[role.Name].Add(roleClaim.Value);
                }
            }
            return Ok(rolesWithPermissions);
        }

    }
}
