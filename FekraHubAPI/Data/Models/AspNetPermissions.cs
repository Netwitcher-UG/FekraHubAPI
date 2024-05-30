using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class AspNetPermissions
    {
        [Key]
        public int Id { get; set; }
       
        public string type { get; set; }

        public string value { get; set; }

       
      


    }
}
