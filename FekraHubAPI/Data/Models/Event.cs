using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }


        [ForeignKey("TypeID")]
        public virtual EventType EventType { get; set; }
        public int? TypeID { get; set; }

        public ICollection<CourseSchedule> CourseSchedule { get; set; }


    }
}
