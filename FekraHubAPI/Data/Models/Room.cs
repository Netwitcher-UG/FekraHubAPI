using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }

       

        [ForeignKey("LocationID")]
        public virtual Location Location { get; set; }
        public int LocationID { get; set; }

        public ICollection<Course> Course { get; set; }


    }
}
