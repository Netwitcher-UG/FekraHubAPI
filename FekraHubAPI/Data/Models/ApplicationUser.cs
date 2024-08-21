using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace FekraHubAPI.Data.Models 
{ 
    public class ApplicationUser : IdentityUser
    {

        public string? Name { get; set; }
        

        public string? ImageUser { get; set; }
        public bool ActiveUser { get; set; } = true;



        [StringLength(50)]
        public string Gender { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }


        public string? EmergencyPhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }  // 1/1/1985 12:00:00 AM
        public string Birthplace { get; set; }

        public string Nationality { get; set; }
        public string Street { get; set; }
        public string StreetNr { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Job { get; set; }
        public string Graduation { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public virtual Tokens Token { get; set; }


        public ICollection<Student> Student { get; set; }
        public ICollection<TeacherAttendance> TeacherAttendance { get; set; }
        public ICollection<Course> Course { get; set; }
        public ICollection<Report> Report { get; set; }
        public ICollection<WorkContract> WorkContract { get; set; }
        public ICollection<PayRoll> PayRoll { get; set; }
    }
}
