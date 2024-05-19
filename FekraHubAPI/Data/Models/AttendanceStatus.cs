using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class AttendanceStatus
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string Title { get; set; }

        public ICollection<TeacherAttendance> TeacherAttendance { get; set; }
    }
}
