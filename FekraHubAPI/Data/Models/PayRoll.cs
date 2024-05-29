using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class PayRoll
    {
        [Key]
        public int Id { get; set; }

        public byte[] File { get; set; }

        public DateTime Timestamp { get; set; }

      
        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; }
        public string? UserID { get; set; }

    }
}
