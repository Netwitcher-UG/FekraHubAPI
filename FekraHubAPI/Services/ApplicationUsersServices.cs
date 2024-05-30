using FekraHubAPI.Data.Models;
using FekraHubAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Seeds;

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

    /*public async Task<List<IdentityUser>> GetAllNonAdminUsersAsync()
    {
        var adminRoleId = await _context.Roles
            .Where(r => r.Name == DefaultRole.Admin)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var adminUsers = _context.UserRoles
            .Where(ur => ur.RoleId == adminRoleId)
            .Select(ur => ur.UserId);

        var nonAdminUsers = await _context.Users
            .Where(u => !adminUsers.Contains(u.Id))
            .ToListAsync();

        return nonAdminUsers;
    }*/

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
