namespace FekraHubAPI.Data.Models
{
    public class UserMessage
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int MessageSenderId { get; set; }
        public MessageSender MessageSender { get; set; }
    }
}
