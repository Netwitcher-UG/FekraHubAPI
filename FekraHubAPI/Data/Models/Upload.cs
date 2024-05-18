using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class Upload
    {
        [Key]
        public int Id { get; set; }
        
        public byte file { get; set; }

        public UploadType UploadTypeID { get; set; }

        public ICollection<Course> Courses { get; set; }
    }
}
