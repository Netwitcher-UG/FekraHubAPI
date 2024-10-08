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
using FekraHubAPI.Constract;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;
using MailKit.Net.Smtp;
using MimeKit;

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
        private readonly ILogger<AccountController> _logger;
        public AccountController(ApplicationDbContext context  , UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager , EmailSender.IEmailSender emailSender
            , IConfiguration configuration 
            , IRepository<SchoolInfo> schoolInfoRepo,
            ApplicationDbContext db,ILogger<AccountController> logger)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _configuration = configuration;
            _emailSender = emailSender;
            _schoolInfoRepo = schoolInfoRepo;
            _logger = logger;
        }

        //[AllowAnonymous]
        //[HttpPost("teeest")]
        //public async Task<IActionResult> test(IFormFile imagePNG)
        //{
        //    string LogoBase64 = "";
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        imagePNG.CopyTo(memoryStream);
        //        byte[] fileBytes = memoryStream.ToArray();
        //        LogoBase64 = Convert.ToBase64String(fileBytes);
        //    }
        //    var OldSchoolInfo = (await _schoolInfoRepo.GetAll()).First();
        //    OldSchoolInfo.LogoBase64 = LogoBase64;
        //    await _schoolInfoRepo.Update(OldSchoolInfo);
        //    return Ok("Erfolg");//Success
        //}
        [HttpGet]
        public async Task<IActionResult> GetAccount()
        {
            var UserId = _schoolInfoRepo.GetUserIDFromToken(User);
            var user = await _db.ApplicationUser.FindAsync(UserId);
            if (user == null)
            {
                return BadRequest($"Benutzer nicht gefunden.");//user not found!
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
            var normalizedUserName = accountUpdate.Email.Normalize().ToLower();

            account.UserName = accountUpdate.Email;
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

                var domain = await _schoolInfoRepo.GetRelationSingle(
                    selector: x=>x.UrlDomain,
                    returnType:QueryReturnType.Single,
                    asNoTracking:true);
                var restPaswordLink = "/reset-password";
                var callbackUrlLink = $"{domain}/{restPaswordLink}?Email={user.Email}&Token={encodedToken}";
                await _emailSender.SendRestPassword(user.Email, callbackUrlLink);
                return Ok();

            }
            return BadRequest($"{email} ist nicht registriert!");//{email} is not registered !
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
                    new Response { Status = "Success", Message = $"Passwort wurde ge�ndert." });//Password has been changed
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = $"Passwort konnte nicht ge�ndert werden, bitte versuchen Sie es erneut." });//Could not change password , please try again.

        }

       
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> LogIn([FromForm] Map_Login login)
        {

            try
            {
                if (ModelState.IsValid)
                {


                    ApplicationUser? user = await _userManager.FindByEmailAsync(login.email);
                    if (user == null || !(await _userManager.CheckPasswordAsync(user, login.password)))
                    {
                        return Unauthorized("E-Mail oder Passwort ist ung�ltig.");//Email or password is invalid
                    }

                    if (!user.ActiveUser)
                    {
                        return BadRequest("Ihr Konto ist nicht aktiv, bitte wenden Sie sich an den Administrator.");//Your account is not active , Contact administrator
                    }
                    if (!user.EmailConfirmed)
                    {
                        
                        await _emailSender.SendConfirmationEmail(user);
                        return StatusCode(409, "Ihr Konto wurde nicht best�tigt. Der Best�tigungslink wurde an Ihre E-Mail gesendet.");//Your account not confirmed . The confirm link has been sent to your email
                    }

                    var claims = new List<Claim>
                        {
                            new Claim("name", user.UserName),
                            new Claim("id", user.Id),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                    var roles = await _userManager.GetRolesAsync(user);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim("role", role));
                        var roleUser = await _roleManager.FindByNameAsync(role);
                        var roleClaims = await _roleManager.GetClaimsAsync(roleUser);
                        foreach (var roleClaim in roleClaims)
                        {
                            claims.Add(new Claim("Permissions", roleClaim.Value));
                        }
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            claims: claims,
                            issuer: _configuration["JWT:Issuer"],
                            audience: _configuration["JWT:Audience"],
                            expires: DateTime.Now.AddMonths(1),
                            signingCredentials: signingCredentials
                        );

                        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                        var userToken = await _db.Token.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                        if (userToken == null)
                        {
                            userToken = new Tokens
                            {
                                Email = user.Email,
                                ExpiryDate = DateTime.Now.AddMonths(1),
                                UserId = user.Id,
                                Token = tokenString
                            };
                            _db.Token.Add(userToken);
                        }
                        else
                        {
                            userToken.Token = tokenString;
                            _db.Token.Update(userToken);
                        }
                        await _db.SaveChangesAsync();


                        return Ok(new { UserData = new { user.FirstName, user.LastName, user.Email }, Role = roles[0].ToString(), token = tokenString, token.ValidTo });
                    
                }
                return BadRequest(ModelState);
               
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AccountController", ex.Message));
                return BadRequest(ex.Message);
            }
            
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
                return BadRequest($"{user.email} ist bereits vergeben.");//Email {user.email} is already token.
            }

            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    string RoleParent = DefaultRole.Parent;
                    if (ModelState.IsValid)
                    {
                        var normalizedEmail = user.email.ToUpperInvariant();
                        var normalizedUserName = user.email.ToUpperInvariant();
                        ApplicationUser appUser = new()
                        {
                            UserName = user.email,
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
                                
                                await _emailSender.SendConfirmationEmail(ThisNewUser);
                                return Ok($"Erfolg!! Bitte gehen Sie zu Ihrem E-Mail-Postfach und best�tigen Sie Ihre E-Mail.");//Success!! . Please go to your email message box and confirm your email
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
                return BadRequest("Ung�ltiger oder abgelaufener Link.");//Invalid or expired link.
            }
            var user = await _userManager.FindByIdAsync(ID);
            if (user == null)
            {
                return BadRequest("Benutzer nicht gefunden.");//User not found.
            }
            if(user.EmailConfirmed == true)
            {
                return Ok();
            }
            Token = Token.Replace(" ", "+");
            var result = await _userManager.ConfirmEmailAsync(user, Token);
            if (result.Succeeded)
            {
                await _emailSender.SendToAdminNewParent(user);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
            
        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> ValidateToken()
        {

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token ist erforderlich.");//Token is required
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
                var userId = principal.FindFirstValue("id");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Ung�ltiger Token: Benutzer-ID fehlt.");//Invalid token: Missing user ID
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var isTokenExists = await _db.Token.Where(x => x.Email == user.Email).FirstOrDefaultAsync();
                    if (isTokenExists != null && isTokenExists.Token == token)
                    {
                        return Ok(new { UserData = new { user.FirstName, user.LastName, user.Email }, validatedToken.ValidTo });
                    }
                    else
                    {
                        return Unauthorized("Ung�ltiger Token.");//Invalid token
                    }
                }
                else
                {
                    return Unauthorized("Ung�ltiger Token.");//Invalid token
                }

            }
            catch (SecurityTokenException)
            {
                return Unauthorized("Ung�ltiger Token.");//Invalid token
            }
        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenData = await _db.Token.Where(x => x.UserId == userId).FirstOrDefaultAsync();
                if (tokenData == null) 
                {
                    return Unauthorized();
                }
                tokenData.Token = "";
                _db.Token.Update(tokenData);
                await _db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
        }
    }

}
