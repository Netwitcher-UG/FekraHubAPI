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
  
        public TimeSpan StartTime { get; set; }  
        public TimeSpan EndTime { get; set; }




        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }
  
        public ICollection<Event> Event { get; set; }
    }
}
