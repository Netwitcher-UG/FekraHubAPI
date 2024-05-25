using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class Upload
    {
        [Key]
        public int Id { get; set; }
        
        public byte[] file { get; set; }

     

        [ForeignKey("UploadTypeID")]
        public virtual UploadType UploadType { get; set; }
        public int? UploadTypeID { get; set; }



        public ICollection<UploadCourse> UploadCourses { get; set; }
    }
}
