using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using FekraHubAPI.HttpRequests.Users;
using FekraHubAPI.Seeds;
using FekraHubAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersManagment : ControllerBase
    {
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public UsersManagment(ApplicationDbContext context  , UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager ,
            ApplicationDbContext db)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var allUsers = await _db.ApplicationUser.ToListAsync();
            return Ok(allUsers);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _db.ApplicationUser.SingleOrDefaultAsync(x => x.Id == id);
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
                        LastName = user.lastname ,
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
            var user = await GetCurrentUserAsync();
            if (user.Id == id)
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

        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> RestPassword(string id , Account accountUpdate)
        {
            var user = await _db.ApplicationUser.FindAsync(id);
            
            if (user == null)
            {
                return NotFound($"user id not exists !");
            }

            await _userManager.CreateAsync(user, accountUpdate.password);
            await _db.SaveChangesAsync();

            return Ok("Password User Reset");
        }
    }
}
