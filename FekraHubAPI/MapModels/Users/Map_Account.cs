using System.ComponentModel.DataAnnotations;


namespace FekraHubAPI.MapModels.Users
{
    public class Map_Account
    {
        [Required]
        public string Role { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [Required]
        public string Email { get; set; }

        public string? PhoneNumber { get; set; }

        public IFormFile? ImageUser { get; set; }
        public bool ActiveUser { get; set; } = false;
        public string? Gender { get; set; }

        public string? EmergencyPhoneNumber { get; set; }
        public DateTime Birthday { get; set; } = DateTime.MinValue;
        public string? Birthplace { get; set; }

        public string? Nationality { get; set; }
        public string? Street { get; set; }
        public string? StreetNr { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? Job { get; set; }
        public string? Graduation { get; set; }


    }


}

