using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class TeacherAttendance
    {
        public int Id { get; set; }
        public DateTime date { get; set; }

        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }

        [ForeignKey("TeacherID")]
        public virtual ApplicationUser Teacher { get; set; }
        public string? TeacherID { get; set; }

        [ForeignKey("StatusID")]
        public virtual AttendanceStatus AttendanceStatus { get; set; }
        public int? StatusID { get; set; }


    }
}
