using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Data.Models
{
     [Index(nameof(Value), IsUnique = true)]

    public class AspNetPermissions
    {
        [Key]
        public int Id { get; set; }
       
        public string Type { get; set; }
        public string  Value { get; set; }

       
      


    }
}
