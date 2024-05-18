using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class CourseEvent
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ScheduleID")]
        public virtual CourseSchedule CourseSchedule { get; set; }
        public int? ScheduleID { get; set; }

        [ForeignKey("EventID")]
        public virtual Event Event { get; set; }
        public int? EventID { get; set; }


    }
}
