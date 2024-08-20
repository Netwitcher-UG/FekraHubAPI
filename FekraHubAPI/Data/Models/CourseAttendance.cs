using System.Security.Claims;

namespace FekraHubAPI.Data.Models
{
    public class CourseAttendance
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public int AttendanceDateId { get; set; }
        public AttendanceDate AttendanceDate { get; set; }
    }
}
