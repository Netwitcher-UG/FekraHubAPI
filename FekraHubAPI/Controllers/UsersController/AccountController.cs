using Microsoft.AspNetCore.Http;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.HttpRequests.Users;
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

namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext context  , UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager ,
            ApplicationDbContext db)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        [HttpGet]

        [HttpGet()]
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
        public async Task<IActionResult> UpdateAccount( [FromForm] Account accountUpdate)
        {
            var getCurrentAccount = await GetCurrentUserAsync();
            var account = await _db.ApplicationUser.FindAsync(getCurrentAccount.Id);
            
            
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
            account.FirstName = accountUpdate.firstName;
            account.LastName = accountUpdate.lastname;
            account.NormalizedUserName = normalizedUserName;
            account.NormalizedEmail = normalizedEmail;

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
        [HttpPost("[action]")]
        public async Task<IActionResult> RestPassword( Account accountUpdate)
        {
            var getCurrentAccount = await GetCurrentUserAsync();
            var account = await _db.ApplicationUser.FindAsync(getCurrentAccount.Id);

            if (account == null)
            {
                return NotFound($"user id not exists !");
            }
            await _userManager.CreateAsync(account, accountUpdate.password);

            return Ok("Password User Reset");
        }
    }
}
