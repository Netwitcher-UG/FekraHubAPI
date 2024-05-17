using Microsoft.AspNetCore.Identity;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Seeds;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FekraHubAPI.Models;

namespace FekraHubAPI.Seeds
{
    public static class DefaultUser
    {
        public static ILookupNormalizer normalizer;
        public static async Task SeedAdminAsync(ModelBuilder builder)
        {
            var password = "123456789";
            var email = "admin@admin.com";

            var normalizedEmail =  email.Normalize().ToLower();
            ApplicationUser user = new ApplicationUser()
            {
                UserName = "Admin",
                Name = "admin",
                NormalizedUserName = "ADMIN",
                Email = email,
                NormalizedEmail = normalizedEmail,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PhoneNumber = "+1234567890"
            };

            user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, password);

            builder.Entity<ApplicationUser>().HasData(user);
            //builder.Entity<IdentityRole>().

            builder.Entity<IdentityUserRole<string>>()
                .HasData(new IdentityUserRole<string>
                {
                    RoleId = "1",
                    UserId = user.Id
                });

        }

        // test
        public static async Task SeedAdminAsync1(UserManager<IdentityUser> userManager)
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

            var user = await userManager.FindByEmailAsync(DefaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(DefaultUser , "123456789");
                //await userManager.AddToRolesAsync(DefaultUser , new List<string> { DefaultRole.Admin });
            }
        }

    }
}
