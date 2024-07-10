using Microsoft.AspNetCore.Http;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.MapModels.Response;
using FekraHubAPI.MapModels.Users;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using FekraHubAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FekraHubAPI.EmailSender;
using Microsoft.AspNetCore.Identity.UI.Services;
using static System.Net.Mime.MediaTypeNames;
using static Azure.Core.HttpHeader;
using System.Security.Cryptography;
using System.IO;
using System.Reflection.Emit;
using System.ComponentModel.DataAnnotations;
using FekraHubAPI.EmailSender;
using Microsoft.Extensions.Configuration;
using static System.Net.WebRequestMethods;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Net; 


namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly EmailSender.IEmailSender _emailSender;
        public AccountController(ApplicationDbContext context  , UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager , EmailSender.IEmailSender emailSender
            , IConfiguration configuration 
            , IRepository<SchoolInfo> schoolInfoRepo,
            ApplicationDbContext db)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _configuration = configuration;
            _emailSender = emailSender;
            _schoolInfoRepo = schoolInfoRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccount()
        {
            var getCurrentAccount = await GetCurrentUserAsync();
            var user = await _db.ApplicationUser.FindAsync(getCurrentAccount.Id);
            if (user == null)
            {
                return NotFound($"user not exists!");
            }
            return Ok(user);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAccount( [FromForm] Map_Account accountUpdate)
        {
            var getCurrentAccount = await GetCurrentUserAsync();
            var account = await _db.ApplicationUser.FindAsync(getCurrentAccount.Id);
            
            
            if (accountUpdate.ImageUser != null)
            {
                using var stream = new MemoryStream();
                await accountUpdate.ImageUser.CopyToAsync(stream);
                account.ImageUser = stream.ToString();

            }
            
            var normalizedEmail = accountUpdate.Email.Normalize().ToLower();
            var normalizedUserName = accountUpdate.UserName.Normalize().ToLower();

            account.UserName = accountUpdate.UserName;
            account.Email = accountUpdate.Email;
            account.FirstName = accountUpdate.FirstName;
            account.LastName = accountUpdate.LastName;
            account.NormalizedUserName = normalizedUserName;
            account.NormalizedEmail = normalizedEmail;

            account.SecurityStamp = Guid.NewGuid().ToString("D");
            account.PhoneNumber = accountUpdate.PhoneNumber;
            account.Gender = accountUpdate.Gender;
            account.EmergencyPhoneNumber = accountUpdate.EmergencyPhoneNumber;
            account.Birthday = accountUpdate.Birthday;
            account.Birthplace = accountUpdate.Birthplace;
            account.Nationality = accountUpdate.Nationality;
            account.Street = accountUpdate.Street;
            account.StreetNr = accountUpdate.StreetNr;
            account.ZipCode = accountUpdate.ZipCode;
            account.City = accountUpdate.City;
            account.Job = accountUpdate.Job;
            account.Graduation = accountUpdate.Graduation;

            _db.SaveChanges();
            return Ok(account);
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("[action]")]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);
                // var callbackUrl = Url.Action("GetResetPassword", "Account", new { email = user.Email, token = token }, protocol: HttpContext.Request.Scheme);

                var domain = (await _schoolInfoRepo.GetRelation()).Select(x=>x.UrlDomain).First();
                var restPaswordLink = "/reset-password";
                var callbackUrlLink = $"{domain}/{restPaswordLink}?Email={user.Email}&Token={encodedToken}";

                await _emailSender.SendRestPassword(user.Email, callbackUrlLink);
                return Ok();

            }
            return BadRequest($"{email} is not registered !");
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("[action]")]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user != null)
            {
                var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
                if (!resetPassResult.Succeeded)
                {
                    foreach (var error in resetPassResult.Errors)
                    {

                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest();
                }
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdatePassword(ChangePassword changePassword)
        {
            var currentUser = await GetCurrentUserAsync();

            var token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);


            if (currentUser != null)
            {

                var resetPassResult = await _userManager.ResetPasswordAsync(currentUser, token, changePassword.Password);
                if (!resetPassResult.Succeeded)
                {
                    foreach (var error in resetPassResult.Errors)
                    {

                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return Ok(ModelState);
                }
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"Password has been changed" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = $"Could not change password , please try again." });

        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> LogIn([FromForm] Map_Login login)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(login.email);
                if (user == null || !(await _userManager.CheckPasswordAsync(user, login.password)))
                {
                    return Unauthorized("Email or password is invalid");
                }

                if (!user.ActiveUser)
                {
                    return BadRequest("You Must Active You Account");
                }
                if (!user.EmailConfirmed)
                {
                    return BadRequest("You Must Confirm You Account");
                }
                var claims = new List<Claim>();
                //claims.Add(new Claim("name", "value"));
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                    var roleUser = await _roleManager.FindByNameAsync(role.ToString());
                    var roleClaims = await _roleManager.GetClaimsAsync(roleUser);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(new Claim("Permissions", roleClaim.Value));
                    }
                }
                //signingCredentials
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                var sc = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    claims: claims,
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    expires: DateTime.Now.AddMonths(1),
                    signingCredentials: sc
                    );
                var Token = new JwtSecurityTokenHandler().WriteToken(token);
               
                return Ok(new {UserData = new { user.FirstName, user.LastName, user.Email }, Token, token.ValidTo });
            }
            return BadRequest(ModelState);
        }
        [AllowAnonymous]
        [HttpPost("RegisterParent")]
        public async Task<IActionResult> RegisterParent([FromForm] Map_RegisterParent user)
        {
            var img = "";
            if (user.imageUser != null && user.imageUser.Length != 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await user.imageUser.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    img = Convert.ToBase64String(imageBytes);
                }
            }
            var IsEmailExists = await _userManager.FindByEmailAsync(user.email);
            if (IsEmailExists != null)
            {
                return BadRequest($"Email {user.email} is already token.");
            }

            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    string RoleParent = DefaultRole.Parent;
                    if (ModelState.IsValid)
                    {
                        var normalizedEmail = user.email.ToUpperInvariant();
                        var normalizedUserName = user.userName.ToUpperInvariant();
                        ApplicationUser appUser = new()
                        {
                            UserName = user.userName,
                            Email = user.email,
                            NormalizedUserName = normalizedUserName,
                            FirstName = user.firstName,
                            LastName = user.lastname,
                            ImageUser = img,
                            NormalizedEmail = normalizedEmail,
                            SecurityStamp = Guid.NewGuid().ToString("D"),
                            PhoneNumber = user.phoneNumber,
                            Gender = user.gender,
                            EmergencyPhoneNumber = user.emergencyPhoneNumber,
                            Birthday = user.birthday ,
                            Birthplace = user.birthplace,
                            Nationality = user.nationality,
                            Street = user.street,
                            StreetNr = user.streetNr,
                            ZipCode = user.zipCode,
                            City = user.city,
                            Job = user.job,
                            Graduation = user.graduation,
                        };

                        IdentityResult result = await _userManager.CreateAsync(appUser, user.password);



                        if (result.Succeeded)
                        {
                            _userManager.AddToRoleAsync(appUser, RoleParent).Wait();
                            transaction.Commit();
                            ApplicationUser? ThisNewUser = await _userManager.FindByEmailAsync(user.email);
                            if (ThisNewUser != null)
                            {
                                var res = await _emailSender.SendConfirmationEmail(ThisNewUser);
                                return Ok($"Success!! . Please go to your email message box and confirm your email");
                            }

                           
                        }
                        else
                        {
                            //foreach (var item in result.Errors)
                            //{
                            //    ModelState.AddModelError("", item.Description);
                            //}
                            return BadRequest(result.Errors.Select(x => x.Description).FirstOrDefault());
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
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ResendConfirmEmail(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null) 
            {
                await _emailSender.SendConfirmationEmail(user);

                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmUser(string Token, string ID)
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(Token))
            {
                return BadRequest("Invalid or expired link.");
            }
            var user = await _userManager.FindByIdAsync(ID);
            if (user != null)
            {
                Token = Token.Replace(" ", "+");
                var result = await _userManager.ConfirmEmailAsync(user, Token);
                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            
            return BadRequest("The User isn't registered.");
        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> ValidateToken()
        {

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is required");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"])),
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out var validatedToken);
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    return Ok(new { UserData = new { user.FirstName, user.LastName, user.Email }, validatedToken.ValidTo });
                }
                else
                {
                    return Unauthorized("Invalid token");
                }
            }
            catch (SecurityTokenException)
            {
                return Unauthorized("Invalid token");
            }
        }
    }

}
