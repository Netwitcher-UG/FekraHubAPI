using System.ComponentModel.DataAnnotations;


namespace FekraHubAPI.HttpRequests.Users
{
    public class AccountUpdate
    {
      

        public string? userName { get; set; }

        public string? firstName { get; set; }
        public string? lastname { get; set; }
        public string? email { get; set; }

        public string? phoneNumber { get; set; }

         public IFormFile? imageUser { get; set; }
         public string? gender { get; set; }

         public string? emergencyPhoneNumber { get; set; }
         public DateTime birthday { get; set; }   = DateTime.MinValue;
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

