namespace FekraHubAPI.Data.Models
{
    public class AttendanceDate
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public ICollection<CourseAttendance> CourseAttendance { get; set; } = new List<CourseAttendance>();

    }
}
