using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.Seeds;
using FekraHubAPI.Data;
using Microsoft.EntityFrameworkCore.Storage;
using FekraHubAPI.MapModels.Users;

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
        private readonly EmailSender.IEmailSender emailSender;

        public LoginRegisterController(ApplicationDbContext context , UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, EmailSender.IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.context = context;
            this.emailSender = emailSender;
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
                            claims.Add(new Claim("type", "value"));
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

        [HttpPost("RegisterParent")]
        public async Task<IActionResult> RegisterParent(RegisterParent user)
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
                            ApplicationUser? ThisNewUser = await userManager.FindByEmailAsync(user.email);
                            if (ThisNewUser != null)
                            {
                                var res = await emailSender.SendConfirmationEmail(ThisNewUser);
                                if (res is OkResult)
                                {
                                    userManager.AddToRoleAsync(appUser, RoleParent).Wait();
                                    transaction.Commit();
                                    return Ok("Success!! . Please go to your email message box and confirm your email");
                                }
                                else
                                {
                                    await userManager.DeleteAsync(ThisNewUser);
                                    return BadRequest("Change your email please!");
                                }
                            }
                            else
                            {
                                return Ok("Resend Link");
                            }
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
        [Route("/NewUser/Confirm")]
        [HttpGet]
        public async Task<string> ConfirmEmail(string token, string ID)
        {
            if (ID == null || token == null)
            {
                return "Link expired";
            }
            var user = await userManager.FindByIdAsync(ID);
            if (user == null)
            {
                return "User not Found";
            }
            else
            {
                token = token.Replace(" ", "+");
                var result = await userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return "Thank you for confirming your email";
                }
                else
                {
                    return "Email not confirmed";
                }
            }
        }
    }
}
