using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class TeacherCourse
    {
        [Key]
        public int Id { get; set; }



        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; }
        public string? UserID { get; set; }


        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }

    }
}
