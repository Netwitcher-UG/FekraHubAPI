using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class StudentAttendance
    {
        public int Id { get; set; }
        public DateTime date { get; set; }

        public ICollection<AttendanceStatus> AttendanceStatus { get; set; }
        public ICollection<Student> Student { get; set; }
        public ICollection<Course> Course { get; set; }
    }
}
