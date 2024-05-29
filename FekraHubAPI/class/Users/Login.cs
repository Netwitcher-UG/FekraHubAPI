using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.@class.Users
{
    public class Login
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string email { get; set; }



        [Required]
        public string password { get; set; }
    }
}
