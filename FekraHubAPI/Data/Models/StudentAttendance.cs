using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Data.Models
{
    public class StudentAttendance
    {
        public int Id { get; set; }
        public DateTime date { get; set; }

        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }

        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; }
        public int? StudentID { get; set; }

        [ForeignKey("StatusID")]
        public virtual AttendanceStatus AttendanceStatus { get; set; }
        public int? StatusID { get; set; }


    
    }
}
