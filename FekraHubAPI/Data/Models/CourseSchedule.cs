using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class CourseSchedule
    {
        [Key]
        public int Id { get; set; }
       
        public string DayOfWeek { get; set; }
  
        public TimeSpan StartTime { get; set; } //= TimeSpan.Now;
        public TimeSpan EndTime { get; set; }// = DateTime.Now;




        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }


        public ICollection<CourseEvent> courseEvent {  get; set; }
    }
}
