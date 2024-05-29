using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FekraHubAPI.HttpRequests.Response;
using FekraHubAPI.Seeds;
using FekraHubAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using FekraHubAPI.EmailSender;
using System.Security.Claims;


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
        public UsersManagment(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;


        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var currentUser = await GetCurrentUserAsync();
            string currentUserRolename = ClaimTypes.Role;
            var allUsers = await _db.ApplicationUser.ToListAsync();
           /* if (currentUserRolename != DefaultRole.Admin ){
            }*/


            return Ok(allUsers);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {

            var user = await _db.ApplicationUser.SingleOrDefaultAsync(x => x.Id == id);
            string userRole = User.FindFirstValue(ClaimTypes.Name);
            var currentUser = await GetCurrentUserAsync();
            string currentUserRolename = ClaimTypes.Role;
            if (currentUserRolename != DefaultRole.Admin && userRole== DefaultRole.Admin)
            {
                return BadRequest("Cant Access This User");

            }
            if (user == null)
            {
                return NotFound($"user not exists!");
            }
            return Ok(user);
        }


        [HttpPost]
        public async Task<IActionResult> AddUser([FromForm] Account user)
        {
            var email = user.email;
            var normalizedEmail = email.Normalize().ToLower();
            var normalizedUserName = user.userName.Normalize().ToLower();
            using var stream = new MemoryStream();
            await user.imageUser.CopyToAsync(stream);
            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    ApplicationUser adduser = new ApplicationUser()
                    {
                        UserName = user.userName,
                        NormalizedUserName = normalizedUserName,
                        Email = email,
                        FirstName = user.firstName,
                        LastName = user.lastname,
                        ImageUser = stream.ToString(),
                        NormalizedEmail = normalizedEmail,
                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        PhoneNumber = user.phoneNumber,
                        Gender = user.gender,
                        EmergencyPhoneNumber = user.emergencyPhoneNumber,
                        Birthday = user.birthday,
                        Birthplace = user.birthplace,
                        Nationality = user.nationality,
                        Street = user.street,
                        StreetNr = user.streetNr,
                        ZipCode = user.zipCode,
                        City = user.city,
                        Job = user.job,
                        Graduation = user.graduation,


                    };
                    IdentityResult result = await _userManager.CreateAsync(adduser, user.password);
                    await _db.ApplicationUser.AddAsync(adduser);
                    await _db.SaveChangesAsync();
                    if (DefaultRole.checkRole(user.role))
                    {
                        _userManager.AddToRoleAsync(adduser, user.role).Wait();
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
        public async Task<IActionResult> UpdateUser(string id, [FromForm] Account accountUpdate)
        {
            var account = await _db.ApplicationUser.FindAsync(id);
            if (account == null)
            {
                return NotFound($" account id {id} not exists !");
            }

            if (accountUpdate.imageUser != null)
            {
                using var stream = new MemoryStream();
                await accountUpdate.imageUser.CopyToAsync(stream);
                account.ImageUser = stream.ToString();

            }
            var normalizedEmail = accountUpdate.email.Normalize().ToLower();
            var normalizedUserName = accountUpdate.userName.Normalize().ToLower();

            account.UserName = accountUpdate.userName;
            account.Email = accountUpdate.email;
            account.NormalizedUserName = normalizedUserName;
            account.NormalizedEmail = normalizedEmail;

            account.FirstName = accountUpdate.firstName;
            account.LastName = accountUpdate.lastname;
            account.SecurityStamp = Guid.NewGuid().ToString("D");
            account.PhoneNumber = accountUpdate.phoneNumber;
            account.Gender = accountUpdate.gender;
            account.EmergencyPhoneNumber = accountUpdate.emergencyPhoneNumber;
            account.Birthday = accountUpdate.birthday;
            account.Birthplace = accountUpdate.birthplace;
            account.Nationality = accountUpdate.nationality;
            account.Street = accountUpdate.street;
            account.StreetNr = accountUpdate.streetNr;
            account.ZipCode = accountUpdate.zipCode;
            account.City = accountUpdate.city;
            account.Job = accountUpdate.job;
            account.Graduation = accountUpdate.graduation;

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
