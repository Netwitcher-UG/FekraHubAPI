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
        private readonly ILogger<ManageController> _logger;
        //protected ILookupNormalizer normalizer;
        public  IServiceProvider serviceProvider;

        public ManageController(ApplicationDbContext _context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IServiceProvider serviceProvider, ILogger<ManageController> logger)
        {
            this.userManager = userManager;
            _roleManager = roleManager;
            this.configuration = configuration;
            this.context = _context;
            this.serviceProvider = serviceProvider;
            _logger = logger;
        }

        [Authorize(Policy = "GetUsers")]
        //[Route("/NewUser/Confirm")]

        [HttpGet("[action]")]
        //[Authorize(Roles =  ""+DefaultRole.Admin+","+DefaultRole.Secretariat )]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var Roles = await _roleManager.FindByNameAsync("Admin");
                var role = await _roleManager.FindByNameAsync("Admin");
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                return Ok(roleClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ManageController", ex.Message));
                return BadRequest(ex.Message);
            }
            
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

    /*public async Task<List<T>> WhereByColumnTypeString(string columnName, string column)
    {
        var entityType = typeof(T);
        var propertyInfo = entityType.GetProperty(columnName);

        //var e = (string)propertyInfo.GetValue(entity);
        var ti = await _dbSet.Where(s => columnName == column).ToListAsync();
        return ti;

    }
    public async Task<List<T>> WhereByColumnTypeString1(string columnName, string column)
    {
        var entityType = typeof(T);
        var propertyInfo = entityType.GetProperty(columnName);
        if (propertyInfo == null)
        {
            throw new ArgumentException($"Column {columnName} does not exist.");
        }

        // var getter = GetPropertGetter(typeof(T).ToString(), columnName);

        //var query = _dbSet.Where(entity => (string)propertyInfo.GetValue(entity) == column);

        var query = _dbSet.Where(i => propertyInfo.Equals(column));

        //  var result = _dbSet.Where(x => T.    == column).Take(50);
        return await query.ToListAsync();
            Task<List<T>> WhereByColumnTypeString(string columnName, string column);
        Task<List<T>> WhereByColumnTypeString1(string columnName, string column);
    }*/


}
