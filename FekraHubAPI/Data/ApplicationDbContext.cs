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
    //IdentityDbContext<IdentityUser>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options ) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
            SeedU(builder);
        }

        private  static void SeedRoles(ModelBuilder builder)
        {
            DefaultRole.SeedRoleAsync(builder);
        }

        private static void SeedU(ModelBuilder builder)
        {
            DefaultUser.SeedAdminAsync(builder);

        }



    }
}
