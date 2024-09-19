using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class WorkContract
    {
        [Key]
        public int Id { get; set; }

        public byte[] File { get; set; }
        public string FileName { get; set; }
        public DateTime Timestamp  { get; set; } = DateTime.Now;

      
        [ForeignKey("TeacherID")]
        public virtual ApplicationUser User { get; set; }
        public string? TeacherID { get; set; }
    }
}
