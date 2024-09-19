using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class MessageSender
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public ICollection<UserMessage> UserMessages { get; set; }

    }
}
