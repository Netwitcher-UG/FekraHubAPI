using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("TypeID")]
        public virtual EventType EventType { get; set; }
        public int? TypeID { get; set; }

       public ICollection<CourseEvent> CourseEvent { get; set; }


    }
}
