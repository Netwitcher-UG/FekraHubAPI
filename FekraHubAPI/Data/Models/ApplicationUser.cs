using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models 
{ 
    public class ApplicationUser : IdentityUser
    {

        public string? Name { get; set; }
        

        public string? ImageUser { get; set; }
        public bool? ActiveUser { get; set; }



        [StringLength(50)]
        public string Gender { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }


        public string? EmergencyPhoneNumber { get; set; }
        public DateTime Birthday { get; set; }
        public string Birthplace { get; set; }

        public string Nationality { get; set; }
        public string Street { get; set; }
        public string StreetNr { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Job { get; set; }
        public string Graduation { get; set; }



        public ICollection<Student> Student { get; set; }
        public ICollection<Course> Course { get; set; }
        public ICollection<ParentContract> ParentContract { get; set; }
        public ICollection<Report> Report { get; set; }
        public ICollection<WorkContract> WorkContract { get; set; }
        public ICollection<PayRoll> PayRoll { get; set; }
    }
}
