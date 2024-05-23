using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace FekraHubAPI.Controllers.UsersController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public AccountsController(UserManager<ApplicationUser> userManager,
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
        public async Task<IActionResult> GetUser(id)
        {
            var user = await _db.ApplicationUser.SingleOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound($"user {id} not exists!");
            }
            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> AddUser()
        {
            var cats = await _db.ApplicationUser.ToListAsync();
            return Ok(cats);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateUser()
        {
            var cats = await _db.ApplicationUser.ToListAsync();
            return Ok(cats);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser()
        {
            var cats = await _db.ApplicationUser.ToListAsync();
            return Ok(cats);
        }
    }
}
