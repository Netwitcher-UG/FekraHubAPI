using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class StudentContract
    {
        [Key]
        public int Id { get; set; }
      
        public byte[] File { get; set; }
        
        public DateTime CreationDate { get; set; } = DateTime.Now;




        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; }
        public int? StudentID { get; set; }


    }
}
