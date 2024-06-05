using Microsoft.AspNetCore.Http;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using FekraHubAPI.Data;
using  FekraHubAPI.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Constract;
using Microsoft.Extensions.Options;

namespace FekraHubAPI.Controllers
{

    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class ManageController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;
        protected ILookupNormalizer normalizer;
        public  IServiceProvider serviceProvider;

        public ManageController(ApplicationDbContext _context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            this.userManager = userManager;
            _roleManager = roleManager;
            this.configuration = configuration;
            this.context = _context;
            this.serviceProvider = serviceProvider;
        }

        [Authorize(Policy = "GetUsers")]

        [HttpGet("[action]")]
        //[Authorize(Roles =  ""+DefaultRole.Admin+","+DefaultRole.Secretariat )]
        public async Task<IActionResult> GetUser()
        {
            var Roles = await _roleManager.FindByNameAsync("Admin");
            var role = await _roleManager.FindByNameAsync("Admin");
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            return Ok(roleClaims);
        }

        [HttpGet("[action]")]
        public List<IdentityRole> AllRoles()
        {

            var AllPermissions = Enum.GetValues(typeof(PermissionsEnum.AllPermissions));
            foreach (var permission in AllPermissions)
            {
                var s = permission.ToString();
            }

            /*
             * 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            var userRole = User.FindFirstValue(ClaimTypes.Role); // will give the user's userName

            //var user = await _userManager.FindAsync(EmailID, Password);
            //string rolename = await _userManager.GetClaimsAsync(user.Id);

           var allUsers = await _db.ApplicationUser.ToListAsync();
            string rolse = DefaultRole.AllRolesWithoutAadmin().ToString();
             //  var allUsers = await _userManager.GetUsersInRoleAsync("Parent");


            IList<string> userRoles = UserManager.GetRoles(userId);
            IList<string> userRoles = await UserManager.GetRolesAsync(userId);
            */
            var Roles =  _roleManager.FindByNameAsync("Admin");
            var Role =  _roleManager.GetClaimsAsync;
           
                //_roleManager.Roles.ToList()

                return _roleManager.Roles.ToList();
        }

        /*public async Task<List<IdentityUser>> GetAllNonAdminUsersAsync()
{
    var adminRoleId = await _context.Roles
        .Where(r => r.Name == DefaultRole.Admin)
        .Select(r => r.Id)
        .FirstOrDefaultAsync();

    var adminUsers = _context.UserRoles
        .Where(ur => ur.RoleId == adminRoleId)
        .Select(ur => ur.UserId);

    var nonAdminUsers = await _context.Users
        .Where(u => !adminUsers.Contains(u.Id))
        .ToListAsync();

    return nonAdminUsers;
}*/
    }


}
