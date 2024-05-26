using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FekraHubAPI.Data.Models
{
    public class AttendanceStatus
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string Title { get; set; }
        [JsonIgnore]
        public ICollection<TeacherAttendance> TeacherAttendance { get; set; }
        [JsonIgnore]
        public ICollection<StudentAttendance> StudentAttendance { get; set; }
    }
}
