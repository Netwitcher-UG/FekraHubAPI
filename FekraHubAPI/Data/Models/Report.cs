using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
       
        public string data { get; set; }

        public bool Improved { get; set; }

        public DateTime CreationDate { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public string? UserId { get; set; }


        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        public int? StudentId { get; set; }
       

      


    }
}
