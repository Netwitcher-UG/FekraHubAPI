using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Users
{
    public class Map_Login
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string email { get; set; }



        [Required]
        public string password { get; set; }
    }
}
