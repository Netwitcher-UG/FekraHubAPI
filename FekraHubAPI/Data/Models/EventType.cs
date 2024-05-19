using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class EventType
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string TypeTitle { get; set; }

        public ICollection<Event> Event { get; set; }
    }
}
