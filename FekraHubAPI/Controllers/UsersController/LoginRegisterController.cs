using Microsoft.AspNetCore.Http;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.Models.Users;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using FekraHubAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginRegisterController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;
        protected ILookupNormalizer normalizer;

        public LoginRegisterController(ApplicationDbContext _context , UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.context = _context;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> LogIn(Login login)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await userManager.FindByEmailAsync(login.email);
                if (user != null)
                {
                    if (await userManager.CheckPasswordAsync(user, login.password))
                    {
                        var claims = new List<Claim>();
                        //claims.Add(new Claim("name", "value"));
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        var roles = await userManager.GetRolesAsync(user);
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                        }
                        //signingCredentials
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]));
                        var sc = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            claims: claims,
                            issuer: configuration["JWT:Issuer"],
                            audience: configuration["JWT:Audience"],
                            expires: DateTime.Now.AddHours(1),
                            signingCredentials: sc
                            );
                        var _token = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                        };
                        return Ok(_token);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    ModelState.AddModelError("message", "Email is invalid");
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterParent(Register user)
        {
            using (IDbContextTransaction transaction = this.context.Database.BeginTransaction())
            {
                try
                {
                    string RoleParent = DefaultRole.Parent;
                    if (ModelState.IsValid)
                    {
                        ApplicationUser appUser = new()
                        {
                            UserName = user.userName,
                            Email = user.email,
                        };
                        IdentityResult result = await userManager.CreateAsync(appUser, user.password);
                        if (result.Succeeded)
                        {
                            userManager.AddToRoleAsync(appUser, RoleParent).Wait();
                            transaction.Commit();
                            return Ok("Success");
                        }
                        else
                        {
                            foreach (var item in result.Errors)
                            {
                                ModelState.AddModelError("", item.Description);
                            }
                        }
                    }
                    transaction.Rollback();
                    return BadRequest(ModelState);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BadRequest(ex.Message);

                }

            }
        }
    }
}
