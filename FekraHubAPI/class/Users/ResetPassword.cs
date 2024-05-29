using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.@class.Users
{
    public class ChangePassword
    {
        [Required]
        public string Password { get; set; } = null!;
        [Compare("Password", ErrorMessage = "The password and confirmation, password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

    }
}
