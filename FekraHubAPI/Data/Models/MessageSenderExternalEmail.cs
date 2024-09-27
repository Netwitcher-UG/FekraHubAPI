namespace FekraHubAPI.Data.Models
{
    public class MessageSenderExternalEmail
    {
        public int MessageSenderId { get; set; }
        public MessageSender MessageSender { get; set; }

        public int ExternalEmailId { get; set; }
        public ExternalEmails ExternalEmail { get; set; }
    }
}
