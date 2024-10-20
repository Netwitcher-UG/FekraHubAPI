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
        public string Gender { get; set; }

        public DateTime Birthday { get; set; }
        public string Nationality { get; set; }
        public string Note { get; set; }
        public string? Street { get; set; }
        public string? StreetNr { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public bool ActiveStudent { get; set; } = true;
        public DateTime? CreatedAt { get; set; }

        [ForeignKey("ParentID")]
        public virtual ApplicationUser User { get; set; }
        public string? ParentID { get; set; }

          [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }

        public ICollection<StudentContract> StudentContract { get; set; }

        public ICollection<StudentAttendance> StudentAttendance { get; set; }
        public ICollection<Report> Report { get; set; }
        public ICollection<Invoice> Invoices { get; set; }




    }
}
