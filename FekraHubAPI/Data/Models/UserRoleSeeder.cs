using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Identity;

namespace FekraHubAPI.Data.Models
{
    public static class UserRoleSeeder
    {
            public static async Task InitializeAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
            {
           

                string[] roleNames = { "Admin", "User" };

                IdentityResult roleResult;

                foreach (var role in roleNames)
                {
                    var roleExists = await roleManager.RoleExistsAsync(role);

                    if (!roleExists)
                    {
                        roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                var email = "admin@site.com";
                var password = "Qwerty123!";

                if (userManager.FindByEmailAsync(email).Result == null)
                {
                    var DefaultUser = new ApplicationUser()
                    {
                        Email = "admin@fekrahub.com",
                        UserName = "fekraHub",
                        Name = "fekra hub",
                        ImageUser = "",
                        ActiveUser = true,
                        EmailConfirmed = true,
                    };

                IdentityResult result = userManager.CreateAsync(DefaultUser, password).Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(DefaultUser, "Admin").Wait();
                    }
                }

            }
        }
    
}
