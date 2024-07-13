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
        private  UserManager<ApplicationUser>  currentUser ;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly ApplicationUsersServices _applicationUsersServices;

        public UsersManagment(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IRepository<ApplicationUser> applicationUserRepository , ApplicationUsersServices applicationUsersServices  ,
        ApplicationDbContext db)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _applicationUserRepository = applicationUserRepository;
            _applicationUsersServices = applicationUsersServices;


        }

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
        [HttpPost]
        public async Task<IActionResult> AddUser([FromForm] Map_Account user)
        {
            var email = user.Email;
            var normalizedEmail = email.Normalize().ToLower();
            var normalizedUserName = user.UserName.Normalize().ToLower();

            string image = "";

            if (user.ImageUser != null)
            {
                string folderFile = "images/users/";
                string folder = Guid.NewGuid().ToString() + "_" + user.ImageUser.FileName;
                image = folderFile +folder;
                if (!System.IO.Directory.Exists(folderFile))
                {
                    System.IO.Directory.CreateDirectory(folderFile);
                }
                string serverFolder = Path.Combine(folderFile, folder);
                using var stream = new FileStream(serverFolder, FileMode.Create);
                    user.ImageUser.CopyTo(stream);
            }

            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    ApplicationUser adduser = new ApplicationUser()
                    {
                        UserName = user.UserName,
                        NormalizedUserName = normalizedUserName,
                        Email = email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ImageUser = image,
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
                        Graduation = user.Graduation,


                    };
                    IdentityResult result = await _userManager.CreateAsync(adduser, user.Password);
                    await _db.ApplicationUser.AddAsync(adduser);
                    await _db.SaveChangesAsync();
                    if (DefaultRole.checkRole(user.Role))
                    {
                        _userManager.AddToRoleAsync(adduser, user.Role).Wait();
                    }
                    else
                    {
                        transaction.Rollback();
                        return Ok("Role Not Found");
                    }
                    transaction.Commit();
                    return Ok("User Added Successfully");

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BadRequest(ex.Message);

                }
            }
        }
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
            var normalizedUserName = accountUpdate.UserName.Normalize().ToLower();

            account.UserName = accountUpdate.UserName;
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

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ResetPasswordUser([Required] string id)
        {
            var user = await _userManager.FindByIdAsync(id);

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

    }
}
