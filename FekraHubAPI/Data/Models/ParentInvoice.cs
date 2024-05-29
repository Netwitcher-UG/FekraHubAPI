using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class ParentInvoice
    {
        [Key]
        public int Id { get; set; }

        public byte[] File { get; set; }

        public DateTime Timestamp { get; set; }

       


        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; }
        public int? StudentID { get; set; }
    }
}
