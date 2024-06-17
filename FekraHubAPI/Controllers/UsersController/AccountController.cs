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

namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly EmailSender.IEmailSender _emailSender;
        public AccountController(ApplicationDbContext context  , UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager , EmailSender.IEmailSender emailSender , IConfiguration configuration ,
            ApplicationDbContext db)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _configuration = configuration;
            _emailSender = emailSender;
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
                var forgotPasswordLink = Url.Action(nameof(ResetPassword), "Authentication", new { token, email = user.Email }, Request.Scheme);
                /*var message = new Message(new string[] { user.Email! }, "Forgot Password Link", forgotPasswordLink!);
                _emailService.SendEmail(message);
                
                return StatusCode(StatusCodes.Status2000K,
                new Response { Status = "Success", Message = $"User created & Email Sent to {user.Email} SuccessFully" });
                */
                return Ok(token);

            }
            return StatusCode(StatusCodes.Status400BadRequest,
                   new Response { Status = "Error", Message = $"Could not send link to email , please try again." });
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
                    return Ok(ModelState);
                }
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"Password has been changed" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = $"Could not change password , please try again." });

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
        [HttpPost("[action]")]
        public async Task<IActionResult> LogIn([FromForm] Map_Login login)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(login.email);
                if (user != null)
                {
                    
                    if (!user.ActiveUser)
                    {
                        return BadRequest("You Must Confirm You Account");
                    }
                    if (await _userManager.CheckPasswordAsync(user, login.password))
                    {
                        

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
        public async Task<IActionResult> RegisterParent([FromForm] Map_RegisterParent user)
        {
            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
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
                        IdentityResult result = await _userManager.CreateAsync(appUser, user.password);



                        if (result.Succeeded)
                        {
                            ApplicationUser? ThisNewUser = await _userManager.FindByEmailAsync(user.email);
                            if (ThisNewUser != null)
                            {
                                var res = await _emailSender.SendConfirmationEmail(ThisNewUser, HttpContext);
                                if (res is OkResult)
                                {
                                    _userManager.AddToRoleAsync(appUser, RoleParent).Wait();
                                    transaction.Commit();
                                    return Ok($"Success!! . Please go to your email message box and confirm your email (https://mail.google.com/mail/u/1/#inbox) .");
                                }
                                else
                                {
                                    await _userManager.DeleteAsync(ThisNewUser);
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
        [HttpPost("[action]")]
        public async Task ResendConfirmEmail(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null) 
            {
                await _emailSender.SendConfirmationEmail(user, HttpContext);
            }
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> ConfirmUser(string token, string ID)
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid or expired link.");
            }
            var user = await _userManager.FindByIdAsync(ID);
            if (user != null)
            {
                token = token.Replace(" ", "+");
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return Redirect("https://www.google.com");
                }
            }
            
            return BadRequest("Email confirmation failed.");
        }
    }

}
