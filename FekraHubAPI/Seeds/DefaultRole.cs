using Microsoft.AspNetCore.Identity;
using FekraHubAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Seeds
{
    public static class DefaultRole
    {
        public const String Admin = "Admin";
        public const String Secretariat = "Secretariat";
        public const String Parent = "Parent";
        public const String Teacher = "Teacher";


        public static async Task SeedRoleAsync(ModelBuilder builder)
        {
            var Roles = builder.Entity<IdentityRole>().HasData(
               new IdentityRole() { Name = Admin, NormalizedName = "Admin", ConcurrencyStamp = "1" , Id="1"},
               new IdentityRole() { Name = Secretariat, NormalizedName = "Secretariat", ConcurrencyStamp = "2", Id = "2" },
               new IdentityRole() { Name = Parent, NormalizedName = "Parent", ConcurrencyStamp = "3", Id = "3" },
               new IdentityRole() { Name = Teacher, NormalizedName = "Teacher", ConcurrencyStamp = "4", Id = "4" }
            );



        }
        public static List<string> AllRoles()
        {
            return new List<string>
            {
                Admin,
                Secretariat,
                Parent,
                Teacher,
            };
        }

        public static List<string> AllRolesWithoutAadmin()
        {
            return new List<string>
            {
                Secretariat,
                Parent,
                Teacher,
            };
        }
        public static bool checkRole(string role)
        {
            var allrole = AllRoles();
            return allrole.Contains(role);
        }

    }
}
