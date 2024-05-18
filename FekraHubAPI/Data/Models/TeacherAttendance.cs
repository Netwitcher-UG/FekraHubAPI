using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class TeacherAttendance
    {
        public int Id { get; set; }
        public DateTime date { get; set; }

        public ICollection<AttendanceStatus> AttendanceStatus { get; set; }
        public ICollection<ApplicationUser> User { get; set; }
        public ICollection<Course> Course { get; set; }


    }
}
