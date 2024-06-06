using FekraHubAPI.Data.Models;
using FekraHubAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Seeds;
using System.Security.Claims;

public class ApplicationUsersServices
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationUsersServices(ApplicationDbContext context , UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }


   
    public async Task<List<ApplicationUser>> GetAllNonAdminUsersAsync()
    {
        var users = _userManager.Users.ToList(); // Get all users
        var nonAdminUsers = new List<ApplicationUser>();

        foreach (var user in users)
        {
            if (!await _userManager.IsInRoleAsync(user, DefaultRole.Admin))
            {
                nonAdminUsers.Add(user);
            }
        }

        return nonAdminUsers;
    }
}
