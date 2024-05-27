using System.ComponentModel.DataAnnotations;


namespace FekraHubAPI.HttpRequests.Users
{
    public class RegisterParent
    {

        [Required]
        public string userName { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public string email { get; set; }

        public string? phoneNumber { get; set; }



    }


}

