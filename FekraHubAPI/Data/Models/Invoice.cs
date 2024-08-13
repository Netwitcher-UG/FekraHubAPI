using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public byte[] file { get; set; }

        public string FileName { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("Studentid")]
        public virtual Student Student { get; set; }
        public int? Studentid { get; set; }

    }
}
