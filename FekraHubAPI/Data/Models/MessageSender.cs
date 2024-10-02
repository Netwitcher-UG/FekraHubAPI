using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class MessageSender
    {
        [Key]
        public int Id { get; set; }
        public string? Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public ICollection<UserMessage> UserMessages { get; set; }
        public ICollection<MessageSenderExternalEmail> MessageSenderExternalEmails { get; set; }

    }
}
