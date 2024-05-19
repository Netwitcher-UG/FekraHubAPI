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
  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

       


        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }


        public ICollection<CourseEvent> courseEvent {  get; set; }
    }
}
