using Microsoft.AspNetCore.Http;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.HttpRequests.Users;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using FekraHubAPI.Data;
using  FekraHubAPI.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

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


        [HttpGet("[action]")]
        //[Authorize(Roles =  ""+DefaultRole.Admin+","+DefaultRole.Secretariat )]
        public async Task<IActionResult> GetUser()
        {
            var Roles = await _roleManager.FindByNameAsync("Admin");
            return Ok(Roles);
        }

        [HttpGet("[action]")]

        public List<IdentityRole> AllRoles()
        {
            var Roles =  _roleManager.FindByNameAsync("Admin");

            return _roleManager.Roles.ToList();
        }
    }


}
