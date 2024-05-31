using Microsoft.AspNetCore.Http;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using FekraHubAPI.Data;
using FekraHubAPI.Controllers;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Constract;

namespace FekraHubAPI.Seeds
{
    public static class DefaultPermissions
    {
        public static  async Task SeedClaimsAsync(ModelBuilder builder)
        {
            var modules = Enum.GetValues(typeof(Helper.PermissionModuleNameAdmin));
            var i = 1 ;
            foreach (var module in modules)
            {
                var AllPermissions = Permissions.GeneratePermissionsFromModule(module.ToString());
                foreach (var Permission in AllPermissions)
                {
                    builder.Entity<IdentityRoleClaim<string>>().HasData(
                    new IdentityRoleClaim<string>()
                    {
                        Id = i,
                        RoleId = "1",
                        ClaimType = Permission,
                        ClaimValue = Permission
                    });
                    i++;
                }
                i++;
            }
            modules = Enum.GetValues(typeof(Helper.PermissionModuleNameSecretariat));
            foreach (var module in modules)
            {
                var AllPermissions = Permissions.GeneratePermissionsFromModule(module.ToString());
                foreach (var Permission in AllPermissions)
                {
                    builder.Entity<IdentityRoleClaim<string>>().HasData(
                    new IdentityRoleClaim<string>()
                    {
                        Id = i,
                        RoleId = "2",
                        ClaimType = Permission,
                        ClaimValue = Permission
                    });
                    i++;
                }
                i++;
            }
            modules = Enum.GetValues(typeof(Helper.PermissionModuleNameParent));
            foreach (var module in modules)
            {
                var AllPermissions = Permissions.GeneratePermissionsFromModule(module.ToString());
                foreach (var Permission in AllPermissions)
                {
                    builder.Entity<IdentityRoleClaim<string>>().HasData(
                    new IdentityRoleClaim<string>()
                    {
                        Id = i,
                        RoleId = "3",
                        ClaimType = Permission,
                        ClaimValue = Permission
                    });
                    i++;
                }
                i++;
            }
            modules = Enum.GetValues(typeof(Helper.PermissionModuleNameTeacher));
            foreach (var module in modules)
            {
                var AllPermissions = Permissions.GeneratePermissionsFromModule(module.ToString());
                foreach (var Permission in AllPermissions)
                {
                    builder.Entity<IdentityRoleClaim<string>>().HasData(
                    new IdentityRoleClaim<string>()
                    {
                        Id = i,
                        RoleId = "4",
                        ClaimType = Permission,
                        ClaimValue = Permission
                    });
                    i++;
                }
                i++;
            }
        }
       
    }
}
