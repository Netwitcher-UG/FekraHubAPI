using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class ParentContract
    {
        [Key]
        public int Id { get; set; }
      
        public byte File { get; set; }
        
        public DateTime CreationDate { get; set; }

       


        [ForeignKey("ParentID")]
        public virtual ApplicationUser User { get; set; }
        public string? ParentID { get; set; }


    }
}
