using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Seeds;

using Microsoft.Extensions.Hosting;

namespace FekraHubAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    //DbContext
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options 
            ): base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
            SeedAdminUser(builder);
        }

        private  static void SeedRoles(ModelBuilder builder)
        {
            DefaultRole.SeedRoleAsync(builder);
            DefaultPermissions.SeedClaimsAsync(builder);
        }

        private static void SeedAdminUser(ModelBuilder builder )
        {
            DefaultUser.SeedAdminAsync(builder);


        }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}
