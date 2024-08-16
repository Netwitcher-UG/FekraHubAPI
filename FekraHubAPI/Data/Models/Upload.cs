using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class Upload
    {
        [Key]
        public int Id { get; set; }
        
        public byte[] file { get; set; }

        public string FileName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [ForeignKey("UploadTypeid")]
        public virtual UploadType UploadType { get; set; }
        public int? UploadTypeid { get; set; }


        public ICollection<Course> Courses { get; set; }
    }
}
