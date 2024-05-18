using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }
        [EmailAddress]
        public DateTime Birthday { get; set; }
        public string Nationality { get; set; }
        public string Note { get; set; }

        [ForeignKey("ParentID")]
        public virtual ApplicationUser User { get; set; }
        public string? ParentID { get; set; }


        public ICollection<Report> Report { get; set; }
        public ICollection<ParentInvoice> parentInvoices { get; set; }




    }
}
