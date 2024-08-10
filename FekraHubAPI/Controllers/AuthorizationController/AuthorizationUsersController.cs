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
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace FekraHubAPI.Controllers.AuthorizationController
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthorizationUsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<AspNetPermissions> _repoPermissions;
        public AuthorizationUsersController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IRepository<AspNetPermissions> repoPermissions)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _repoPermissions = repoPermissions;

        }
        [Authorize(Policy = "ManagePermissions")]
        [HttpPost("[action]")]
        public async Task<IActionResult> SchoolPermissions()
        {
            var roles = await _roleManager.Roles
                .Where(x => x.Name != "Parent")
                .ToListAsync();

            var rolesWithPermissions = new Dictionary<string, List<string>>();

            foreach (var role in roles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                rolesWithPermissions[role.Name] = roleClaims.Select(claim => claim.Value).ToList();
            }
            return Ok(rolesWithPermissions);
        }
        [Authorize(Policy = "ManagePermissions")]
        [HttpPost("[action]")]
        public async Task<IActionResult> AllRolesAndPermissions()
        {
            var roles = await _roleManager.Roles
                .Where(x => x.Name != "Parent")
                .ToListAsync();

            var parentRole = await _roleManager.Roles
                .FirstOrDefaultAsync(x => x.Name == "Parent");

            if (parentRole != null)
            {
                var parentClaims = (await _roleManager.GetClaimsAsync(parentRole))
                    .Select(claim => claim.Value)
                    .ToList();

                var permissions = (await _repoPermissions.GetRelation())
                    .Where(permission => !parentClaims.Contains(permission.Value))
                    .Select(permission => permission.Value)
                    .ToList();

                return Ok(new
                {
                    AllRoles = roles.Select(x => x.Name),
                    AllPermissions = permissions
                });
            }

            return NotFound();
        }

        [Authorize(Policy = "ManagePermissions")]
        [HttpPost("[action]")]
        public async Task<IActionResult> AssignPermissionToRole([FromQuery] string RoleName, [FromQuery] List<string> PermissionsName)
        {
            if (string.IsNullOrEmpty(RoleName) || PermissionsName == null || !PermissionsName.Any())
            {
                return BadRequest("RoleName or PermissionsName cannot be null or empty.");
            }

            var role = await _roleManager.FindByNameAsync(RoleName);
            if (role == null)
            {
                return NotFound("Role not found.");
            }
            var validPermissions = PermissionsName.Where(x => PermissionsEnum.CheckPermissionExist(x)).ToList();
            if (validPermissions.Count() != PermissionsName.Count())
            {
                return BadRequest("One ore more permissions not valid");
            }
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var currentPermissions = roleClaims.Select(c => c.Value).ToList();
            var claimsToRemove = roleClaims.Where(c => !validPermissions.Contains(c.Value) && PermissionsEnum.CheckPermissionExist(c.Value)).ToList();
            foreach (var claim in claimsToRemove)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            var claimsToAdd = validPermissions.Where(p => !currentPermissions.Contains(p)).ToList();
            foreach (var permission in claimsToAdd)
            {
                await _roleManager.AddClaimAsync(role, new Claim(permission, permission));
            }

            return Ok("Permissions updated successfully.");

        }
        //[Authorize(Policy = "ManagePermissions")]
        //[HttpPost("[action]")]
        //public async Task<IActionResult> AssignPermissionToRole([FromQuery][Required] string RoleName, [FromQuery][Required] string PermissionName)
        //{
        //    var role = await _roleManager.FindByNameAsync(RoleName);
        //    var roleClaims = await _roleManager.GetClaimsAsync(role);
        //    bool claimExists = roleClaims.Any(c => c.Type == PermissionName && c.Value == PermissionName);

        //    var isTrue = PermissionsEnum.CheckPermissionExist(PermissionName);
        //    // check permission is exist 
        //    if (PermissionsEnum.CheckPermissionExist(PermissionName) && !claimExists)
        //    {
        //        await _roleManager.AddClaimAsync(role, new Claim(PermissionName , PermissionName));
        //        return Ok("Permission Assigned Successfully");
        //    }
        //    return BadRequest("Error In Assign");
        //}
        //[Authorize(Policy = "ManagePermissions")]
        //[HttpPost("[action]")]
        //public async Task<IActionResult> RemovePermissionToRole([FromQuery][Required] string RoleName, [FromQuery][Required] string PermissionName)
        //{
        //    var role = await _roleManager.FindByNameAsync(RoleName);
        //    var roleClaims = await _roleManager.GetClaimsAsync(role);
        //    // check permission is exist 
        //    if (PermissionsEnum.CheckPermissionExist(PermissionName))
        //    {
        //        await _roleManager.RemoveClaimAsync(role, new Claim(PermissionName, PermissionName));
        //        return Ok("Permission Removed Successfully");
        //    }
        //    return BadRequest("Error In Assign");
        //}
        [AllowAnonymous]
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
                rolesWithPermissions[role.Name] = roleClaims.Select(x => x.Value).ToList();
            }
            return Ok(rolesWithPermissions);
        }

    }
}
