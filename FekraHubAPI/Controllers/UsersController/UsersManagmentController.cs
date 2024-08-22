using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using FekraHubAPI.Seeds;
using FekraHubAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using FekraHubAPI.EmailSender;
using System.Security.Claims;
using FekraHubAPI.Repositories.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using System.IO;
using FekraHubAPI.MapModels.Users;
using FekraHubAPI.MapModels.Response;
using Microsoft.AspNetCore.Identity.UI.Services;
using FekraHubAPI.MapModels;


namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersManagment : ControllerBase
    {
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
       // private  UserManager<ApplicationUser>  currentUser ;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly ApplicationUsersServices _applicationUsersServices;
        private readonly EmailSender.IEmailSender _emailSender;
        public UsersManagment(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IRepository<ApplicationUser> applicationUserRepository , ApplicationUsersServices applicationUsersServices  ,
        ApplicationDbContext db, EmailSender.IEmailSender emailSender)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _applicationUserRepository = applicationUserRepository;
            _applicationUsersServices = applicationUsersServices;
            _emailSender = emailSender;


        }
        [Authorize(Policy = "GetUsers")]
        [HttpGet("PaginationParameters")]
        public async Task<IActionResult> PaginationParameters([FromQuery] PaginationParameters paginationParameters)
        {
            var pagedProducts = await _applicationUserRepository.GetPagedDataAsync(await _applicationUserRepository.GetRelation<ApplicationUser>(), paginationParameters);
            return Ok(pagedProducts);
        }

        [Authorize(Policy = "GetUsers")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role); 

            var allUsers = await _db.ApplicationUser.ToListAsync();

            if (userRole != DefaultRole.Admin ){
                allUsers = await _applicationUsersServices.GetAllNonAdminUsersAsync();
            }
            var data = allUsers.Select(x => new
            {
                x.Id,
                x.Name,
                x.UserName,
                x.FirstName,
                x.LastName,
                x.Email,
                x.ImageUser,
                x.Gender,
                x.Job,
                x.Birthday,
                x.Birthplace,
                x.Nationality,
                x.City,
                x.Street,
                x.StreetNr,
                x.ZipCode,
                x.PhoneNumber,
                x.EmergencyPhoneNumber,

            }).ToList();
            return Ok(data);
        }
        [Authorize(Policy = "GetEmployee")]

        [HttpGet("GetEmployee")]
        public async Task<IActionResult> GetEmployee()
        {
            var roleIds = new List<string> { "2", "4" };
            var userIdsInRoles = await _db.UserRoles
                                .Where(x => roleIds.Contains(x.RoleId))
                                .Select(x => x.UserId)
                                .ToListAsync();
            var usersInRoles = await _db.ApplicationUser
                                .Where(x => userIdsInRoles.Contains(x.Id))
                                .ToListAsync();

            var data = usersInRoles.Select(x => new
            {
                x.Id,
                x.Name,
                x.UserName,
                x.FirstName,
                x.LastName,
                x.Email,
                x.ImageUser,
                x.Gender,
                x.Job,
                x.Birthday,
                x.Birthplace,
                x.Nationality,
                x.City,
                x.Street,
                x.StreetNr,
                x.ZipCode,
                x.PhoneNumber,
                x.EmergencyPhoneNumber,

            }).ToList();
            return Ok(data);
        }
        
        [Authorize(Policy = "GetTeacher")]
        [HttpGet("GetTeacher")]
        public async Task<IActionResult> GetTeacher()
        {
            var role =  "4" ;
            var userIdsInRoles = await _db.UserRoles
                                .Where(x => x.RoleId == role)
                                .Select(x => x.UserId)
                                .ToListAsync();
            var usersInRoles = await _db.ApplicationUser
                                .Where(x => userIdsInRoles.Contains(x.Id))
                                .ToListAsync();

            var data = usersInRoles.Select(x => new
            {
                x.Id,
                x.Name,
                x.UserName,
                x.FirstName,
                x.LastName,
                x.Email,
                x.ImageUser,
                x.Gender,
                x.Job,
                x.Birthday,
                x.Birthplace,
                x.Nationality,
                x.City,
                x.Street,
                x.StreetNr,
                x.ZipCode,
                x.PhoneNumber,
                x.EmergencyPhoneNumber,

            }).ToList();
            return Ok(data);
        }

        [Authorize(Policy = "GetSecretary")]
        [HttpGet("GetSecretary")]
        public async Task<IActionResult> GetSecretary()
        {
            var role = "2";
            var userIdsInRoles = await _db.UserRoles
                                .Where(x => x.RoleId == role)
                                .Select(x => x.UserId)
                                .ToListAsync();
            var usersInRoles = await _db.ApplicationUser
                                .Where(x => userIdsInRoles.Contains(x.Id))
                                .ToListAsync();

            var data = usersInRoles.Select(x => new
            {
                x.Id,
                x.Name,
                x.UserName,
                x.FirstName,
                x.LastName,
                x.Email,
                x.ImageUser,
                x.Gender,
                x.Job,
                x.Birthday,
                x.Birthplace,
                x.Nationality,
                x.City,
                x.Street,
                x.StreetNr,
                x.ZipCode,
                x.PhoneNumber,
                x.EmergencyPhoneNumber,

            }).ToList();
            return Ok(data);
        }

        [Authorize(Policy = "GetParent")]
        [HttpGet("GetPerent")]
        public async Task<IActionResult> GetPerent()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var allUsers = await _db.ApplicationUser.ToListAsync();


            if (userRole != DefaultRole.Admin)
            {
                allUsers = await _applicationUsersServices.GetAllNonAdminUsersAsync();
            }

            allUsers = allUsers.Where(x => _userManager.IsInRoleAsync(x, "Parent").Result)
                .ToList();

            var data = allUsers.Select(x => new
            {
                x.Id,
                x.Name,
                x.UserName,
                x.FirstName,
                x.LastName,
                x.Email,
                x.ImageUser,
                x.Gender,
                x.Job,
                x.Birthday,
                x.Birthplace,
                x.Nationality,
                x.City,
                x.Street,
                x.StreetNr,
                x.ZipCode,
                x.PhoneNumber,
                x.EmergencyPhoneNumber,

            }).ToList();
            return Ok(data);
        }
        
        [Authorize(Policy = "GetUsers")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var currentUser = await GetCurrentUserAsync();
            var userRole = User.FindFirstValue(ClaimTypes.Role);  // current role

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"user not exists!");
            }

            var  isAdmin = await _userManager.IsInRoleAsync(user ,DefaultRole.Admin);

            if (userRole != DefaultRole.Admin && isAdmin)
            {
                return BadRequest("Cant Access This User");
            }
            var data = new
            {
                user.Id,
                user.Name,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.Email,
                user.ImageUser,
                user.Gender,
                user.Job,
                user.Birthday,
                user.Birthplace,
                user.Nationality,
                user.City,
                user.Street,
                user.StreetNr,
                user.ZipCode,
                user.PhoneNumber,
                user.EmergencyPhoneNumber,

            };
            return Ok(data);
        }
        [Authorize(Policy = "AddUsers")]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromForm] Map_Account user)
        {
            var img = "";
            if (user.ImageUser != null && user.ImageUser.Length != 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await user.ImageUser.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    img = Convert.ToBase64String(imageBytes);
                }
            }
            var IsEmailExists = await _userManager.FindByEmailAsync(user.Email);
            if (IsEmailExists != null)
            {
                return BadRequest($"Email {user.Email} is already token.");
            }

            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    string RoleParent = DefaultRole.Parent;
                    if (ModelState.IsValid)
                    {
                        var normalizedEmail = user.Email.ToUpperInvariant();
                        var normalizedUserName = user.Email.ToUpperInvariant();
                        ApplicationUser appUser = new()
                        {
                            UserName = user.Email,
                            NormalizedUserName = normalizedUserName,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            ImageUser = img,
                            NormalizedEmail = normalizedEmail,
                            SecurityStamp = Guid.NewGuid().ToString("D"),
                            PhoneNumber = user.PhoneNumber,
                            Gender = user.Gender,
                            EmergencyPhoneNumber = user.EmergencyPhoneNumber,
                            Birthday = user.Birthday,
                            Birthplace = user.Birthplace,
                            Nationality = user.Nationality,
                            Street = user.Street,
                            StreetNr = user.StreetNr,
                            ZipCode = user.ZipCode,
                            City = user.City,
                            Job = user.Job,
                            Graduation = user.Graduation
                        };

                        IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);



                        if (result.Succeeded)
                        {
                            
                            if (DefaultRole.checkRole(user.Role))
                            {
                                _userManager.AddToRoleAsync(appUser, user.Role).Wait();
                                transaction.Commit();
                                await _emailSender.SendConfirmationEmailWithPassword(appUser, user.Password);
                                return Ok();
                            }
                            else
                            {
                                transaction.Rollback();
                                return BadRequest("Role Not Found");
                            }


                        }
                        else
                        {
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
        [Authorize(Policy = "UpdateUser")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] Map_Account accountUpdate)
        {
            var account = await _db.ApplicationUser.FindAsync(id);
            if (account == null)
            {
                return NotFound($" account id {id} not exists !");
            }

            string folderFile = "images/users/";
            if (accountUpdate.ImageUser != null)
            {
                if (account.ImageUser != accountUpdate.ImageUser.FileName)
                {
                    string folder = Guid.NewGuid().ToString() + "_" + accountUpdate.ImageUser.FileName;
                    string image = folderFile + folder;

                    if (!System.IO.Directory.Exists(folderFile))
                    {
                        System.IO.Directory.Delete(folderFile);

                    }
                    string serverFolder = Path.Combine(folderFile, folder);
                    using var stream = new FileStream(serverFolder, FileMode.Create);
                    accountUpdate.ImageUser.CopyTo(stream);
                    account.ImageUser = image;
                    //image = user.imageUser.ToString();
                }

            }



            // await _applicationUserRepository.Update(account);

            var normalizedEmail = accountUpdate.Email.Normalize().ToLower();
            var normalizedUserName = accountUpdate.Email.Normalize().ToLower();

            account.UserName = accountUpdate.Email;
            account.Email = accountUpdate.Email;
            account.NormalizedUserName = normalizedUserName;
            account.NormalizedEmail = normalizedEmail;

            account.FirstName = accountUpdate.FirstName;
            account.LastName = accountUpdate.LastName;
            account.SecurityStamp = Guid.NewGuid().ToString("D");
            account.PhoneNumber = accountUpdate.PhoneNumber;
            account.Gender = accountUpdate.Gender;
            account.EmergencyPhoneNumber = accountUpdate.EmergencyPhoneNumber;
            account.Birthday = accountUpdate.Birthday;
            account.Birthplace = accountUpdate.Birthplace;
            account.Nationality = accountUpdate.Nationality;
            account.Street = accountUpdate.Street;
            account.StreetNr = accountUpdate.Street;
            account.ZipCode = accountUpdate.ZipCode;
            account.City = accountUpdate.City;
            account.Job = accountUpdate.Job;
            account.Graduation = accountUpdate.Graduation;

            _db.SaveChanges();
            return Ok(account);
        }

        [Authorize(Policy = "DeleteUser")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser.Id == id)
            {
                return BadRequest("Cant Delete This User");
            }
            var deleteUser = await _db.ApplicationUser.SingleOrDefaultAsync(x => x.Id == id);
            if (deleteUser == null)
            {
                return NotFound($"user id not exists !");
            }
            _db.ApplicationUser.Remove(deleteUser);
            await _db.SaveChangesAsync();
            return Ok("User Deleted");
        }
        [Authorize(Policy = "ResetPasswordUser")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ResetPasswordUser([Required] string id , [Required] string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPassResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (resetPassResult.Succeeded)
                {
                    return Ok();

                }
                return StatusCode(StatusCodes.Status400BadRequest,
                   new Response { Status = "Error", Message = resetPassResult.ToString()});
                var forgotPasswordLink = Url.Action(nameof(ResetPassword), "Authentication", new { token, email = user.Email }, Request.Scheme);
                /*var message = new Message(new string[] { user.Email! }, "Forgot Password Link", forgotPasswordLink!);
                _emailService.SendEmail(message);
                
                return StatusCode(StatusCodes.Status2000K,
                new Response { Status = "Success", Message = $"User created & Email Sent to {user.Email} SuccessFully" });
                */
                //return Ok(token);

            }
            return StatusCode(StatusCodes.Status400BadRequest,
                   new Response { Status = "Error", Message = $"Could not send link to email , please try again." });
        }

    }
}
