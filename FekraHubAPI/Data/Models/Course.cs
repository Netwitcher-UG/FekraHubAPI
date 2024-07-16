using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public int Price { get; set; }
        public int Lessons { get; set; }
        public int Capacity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        //[ForeignKey("UserId")]
        //public virtual ApplicationUser User { get; set; }
        //public string? UserId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }
        public int? RoomId { get; set; }

        public ICollection<TeacherAttendance> TeacherAttendance { get; set; }
        public ICollection<StudentAttendance> StudentAttendance { get; set; }
        public ICollection<CourseSchedule> CourseSchedule { get; set; }
        public ICollection<Student> Student { get; set; }

        public ICollection<Upload> Upload { get; set; }
   
        public ICollection<ApplicationUser> Teacher { get; set; }

    }
}
