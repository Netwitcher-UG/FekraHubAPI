using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Models.Users
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
