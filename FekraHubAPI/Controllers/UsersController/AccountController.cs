using FekraHubAPI.Constract;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.EmailSender;
using FekraHubAPI.Helpers;
using FekraHubAPI.MapModels.Response;
using FekraHubAPI.MapModels.Users;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Azure.Core.HttpHeader;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

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
            account.Birthday = accountUpdate.Birthday.ToUtcSafe();
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
                var restPaswordLink = "reset-password";
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
                    new Response { Status = "Success", Message = $"Passwort wurde geändert." });//Password has been changed
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = $"Passwort konnte nicht geändert werden, bitte versuchen Sie es erneut." });//Could not change password , please try again.

        }


        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> LogIn([FromForm] Map_Login login)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ApplicationUser? user = await _userManager.FindByEmailAsync(login.email);
                if (user == null || !(await _userManager.CheckPasswordAsync(user, login.password)))
                    return Unauthorized("E-Mail oder Passwort ist ungültig.");

                if (!user.ActiveUser)
                    return BadRequest("Ihr Konto ist nicht aktiv, bitte wenden Sie sich an den Administrator.");

                if (!user.EmailConfirmed)
                {
                    await _emailSender.SendConfirmationEmail(user);
                    return StatusCode(409, "Ihr Konto wurde nicht bestätigt. Der Bestätigungslink wurde an Ihre E-Mail gesendet.");
                }

                // 1) جهّز الـ JTI أولًا عشان نقدر نلوّغها
                var jti = Guid.NewGuid().ToString();

                var claims = new List<Claim>
        {
            new Claim("name", user.UserName),
            new Claim("id", user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles.Where(r => !string.IsNullOrWhiteSpace(r)))
                {
                    claims.Add(new Claim("role", role));

                    var roleUser = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == role);
                    if (roleUser == null)
                    {
                        _logger.LogWarning("Role '{RoleName}' not found while logging in user {UserId}", role, user.Id);
                        continue;
                    }

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
                    expires: DateTime.UtcNow.AddMonths(1),
                    signingCredentials: signingCredentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // ✅ 2) اللوغ هنا بعد ما صار عندنا jti + tokenString + user
                _logger.LogInformation(
                    "Login issued token | Email={Email} | JTI={JTI} | TokenLen={Len}",
                    user.Email, jti, tokenString.Length
                );

                var userToken = await _db.Token.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (userToken == null)
                {
                    userToken = new Tokens
                    {
                        Email = user.Email,
                        ExpiryDate = DateTime.UtcNow.AddMonths(1).ToUtcSafe(),
                        UserId = user.Id,
                        Token = tokenString
                    };
                    _db.Token.Add(userToken);
                }
                else
                {
                    userToken.Token = tokenString;
                    userToken.ExpiryDate = DateTime.UtcNow.AddMonths(1).ToUtcSafe();
                    _db.Token.Update(userToken);
                }

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    UserData = new { user.FirstName, user.LastName, user.Email },
                    Role = roles.FirstOrDefault(),
                    token = tokenString,
                    token.ValidTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
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
                            Birthday = user.birthday.ToUtcSafe(),
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
                                return Ok($"Erfolg!! Bitte gehen Sie zu Ihrem E-Mail-Postfach und bestätigen Sie Ihre E-Mail.");//Success!! . Please go to your email message box and confirm your email
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
                return BadRequest("Ungültiger oder abgelaufener Link.");//Invalid or expired link.
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
            // --------- 0) معلومات تشخيص عامة ----------
            var instance = Environment.MachineName;
            HttpContext.Response.Headers["X-Instance"] = instance;

            var secret = _configuration["JWT:SecretKey"];
            var issuer = _configuration["JWT:Issuer"];
            var audience = _configuration["JWT:Audience"];

            _logger.LogInformation(
                "ValidateToken START | Instance={Instance} | HasSecret={HasSecret} | SecretLen={SecretLen} | Issuer={Issuer} | Audience={Audience}",
                instance,
                !string.IsNullOrWhiteSpace(secret),
                secret?.Length ?? 0,
                issuer,
                audience
            );

            // --------- 1) قراءة الهيدر ----------
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                _logger.LogWarning("ValidateToken FAIL | Instance={Instance} | Reason=NoAuthorizationHeader", instance);
                return Unauthorized("Token ist erforderlich.");
            }

            var token = authHeader;
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token.Substring("Bearer ".Length);

            token = token.Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("ValidateToken FAIL | Instance={Instance} | Reason=EmptyTokenAfterTrim", instance);
                return Unauthorized("Token ist erforderlich.");
            }

            // --------- 2) إعدادات التحقق ----------
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidIssuer = issuer,
                ValidAudience = audience,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                _logger.LogInformation(
                    "ValidateToken JWT OK | Instance={Instance} | ValidTo(UTC)={ValidTo}",
                    instance,
                    validatedToken.ValidTo
                );

                // --------- 3) استخراج userId ----------
                var userId =
                    principal.FindFirstValue("id") ??
                    principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    principal.FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ValidateToken FAIL | Instance={Instance} | Reason=MissingUserIdClaim", instance);
                    return Unauthorized("Ungültiger Token: Benutzer-ID fehlt.");
                }

                // --------- 4) جلب المستخدم ----------
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("ValidateToken FAIL | Instance={Instance} | Reason=UserNotFound | UserId={UserId}", instance, userId);
                    return Unauthorized("Ungültiger Token.");
                }

                // --------- 5) DB token ----------
                var tokenRow = await _db.Token.FirstOrDefaultAsync(x => x.Email == user.Email);
                if (tokenRow == null)
                {
                    _logger.LogWarning("ValidateToken FAIL | Instance={Instance} | Reason=NoTokenRowInDb | Email={Email}", instance, user.Email);
                    return Unauthorized("Ungültiger Token.");
                }

                var dbToken = (tokenRow.Token ?? "").Trim();
                if (dbToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    dbToken = dbToken.Substring("Bearer ".Length).Trim();

                if (!string.Equals(dbToken, token, StringComparison.Ordinal))
                {
                    _logger.LogWarning(
                        "ValidateToken FAIL | Instance={Instance} | Reason=DbTokenMismatch | Email={Email} | DbTokenLen={DbLen} | HeaderTokenLen={HdrLen}",
                        instance, user.Email, dbToken.Length, token.Length
                    );
                    return Unauthorized("Ungültiger Token.");
                }

                _logger.LogInformation("ValidateToken SUCCESS | Instance={Instance} | Email={Email}", instance, user.Email);

                return Ok(new
                {
                    UserData = new { user.FirstName, user.LastName, user.Email },
                    ValidTo = validatedToken.ValidTo
                });
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "ValidateToken FAIL | Instance={Instance} | Reason=TokenExpired", instance);
                return Unauthorized("Ungültiger Token.");
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "ValidateToken FAIL | Instance={Instance} | Reason=InvalidSignature (Secret mismatch?)", instance);
                return Unauthorized("Ungültiger Token.");
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "ValidateToken FAIL | Instance={Instance} | Reason=SecurityTokenException", instance);
                return Unauthorized("Ungültiger Token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateToken FAIL | Instance={Instance} | Reason=UnhandledException", instance);
                return Unauthorized("Ungültiger Token.");
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
