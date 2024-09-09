using System.ComponentModel.DataAnnotations;


namespace FekraHubAPI.MapModels.Users
{
    public class Map_RegisterParent
    {


        [Required]
        public string password { get; set; }

        [Required]
        public string email { get; set; }

        public string? phoneNumber { get; set; }
        public string? firstName { get; set; }
        public string? lastname { get; set; }
        public IFormFile? imageUser { get; set; }
        public bool? activeUser { get; set; } = true;
        public string? gender { get; set; }

        public string? emergencyPhoneNumber { get; set; }
        public DateTime? birthday { get; set; }
        public string? birthplace { get; set; }

        public string? nationality { get; set; }
        public string? street { get; set; }
        public string? streetNr { get; set; }
        public string? zipCode { get; set; }
        public string? city { get; set; }
        public string? job { get; set; }
        public string? graduation { get; set; }
    }


}

