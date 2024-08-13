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

        public static async Task SeedPermissionsAsync(ModelBuilder builder)
        {
            var AllPermissions = Enum.GetValues(typeof(PermissionsEnum.AllPermissions));
            var i = 20;
            foreach (var Permission in AllPermissions)
            {
                builder.Entity<AspNetPermissions>().HasData(
                new AspNetPermissions()
                {
                    Id = i,
                    Type = Permission.ToString(),
                    Value = Permission.ToString()
                });
                i++;
            }
        }
        public static  async Task SeedRoleAdminClaimsAsync(ModelBuilder builder)
        {
            List<string> per2 = new()
                    {
                        "GetStudentsReports",
                        "ApproveReports",
                        "GetUsers",
                        "AddCourse",
                        "GetEmployee",
                        "GetParent",
                        "AddStudentAttendance",
                        "GetCourse"
                    };
            List<string> per4 = new()
                    {
                        "GetStudentsReports",
                        "AddUsers",
                        "InsertUpdateStudentsReports"
                    };
            List<IdentityRoleClaim<string>> identityRoleClaims = new();
            var AllPermissions = Enum.GetValues(typeof(PermissionsEnum.AllPermissions));
            var i = 70;
            foreach (var Permission in AllPermissions)
            {

                if (Permission.ToString() == "ManageChildren")
                {

                    var x3 = new IdentityRoleClaim<string>()
                    {
                        Id = i,
                        RoleId = "3",
                        ClaimType = Permission.ToString(),
                        ClaimValue = Permission.ToString()
                    };
                    identityRoleClaims.Add(x3);
                    i++;
                }
                else

                {
                    
                    if ( per2.Contains(Permission.ToString()))
                    {
                        
                     var x2 = new IdentityRoleClaim<string>()
                     {
                         Id = i,
                         RoleId = "2",
                         ClaimType = Permission.ToString(),
                         ClaimValue = Permission.ToString()
                     };
                        identityRoleClaims.Add(x2);
                        i++;
                    }

                    

                    if (per4.Contains(Permission.ToString()))
                    {
                        
                     var x4 = new IdentityRoleClaim<string>()
                     {
                         Id = i,
                         RoleId = "4",
                         ClaimType = Permission.ToString(),
                         ClaimValue = Permission.ToString()
                     };
                        identityRoleClaims.Add(x4);
                        i++;
                    }


                    var x1 = new IdentityRoleClaim<string>()
                    {
                        Id = i,
                        RoleId = "1",
                        ClaimType = Permission.ToString(),
                        ClaimValue = Permission.ToString()
                    };
                    identityRoleClaims.Add(x1);
                    i++;
                }

               


            }
            builder.Entity<IdentityRoleClaim<string>>().HasData(identityRoleClaims);
            /* modules = Enum.GetValues(typeof(Helper.PermissionModuleNameSecretariat));
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
             */
        }
       
    }
}
