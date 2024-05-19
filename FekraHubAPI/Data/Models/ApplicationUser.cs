using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace FekraHubAPI.Data.Models 
{ 
    public class ApplicationUser : IdentityUser
    {

        public string? Name { get; set; }
        

        public string? ImageUser { get; set; }
        public bool? ActiveUser { get; set; }
    }
}