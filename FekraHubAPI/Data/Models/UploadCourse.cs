using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace FekraHubAPI.Data.Models
{
    public class UploadCourse
    {
        [Key]
        public int Id { get; set; }
      
  

        [ForeignKey("UploadID")]
        public virtual Upload Upload { get; set; }
        public int? UploadID { get; set; }


          [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
        public int? CourseID { get; set; }

    }
}
