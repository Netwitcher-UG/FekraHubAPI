using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Http;

namespace FekraHubAPI.Models.Users
{
    public class Register
    {
        private string RoleParent = DefaultRole.Parent;
        private string RoleTeacher = DefaultRole.Teacher;

        [Required]
        public string userName { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public string email { get; set; }

        public string? phoneNumber { get; set; }



    }

   // public enum RoleName { Admin , pp};

}

