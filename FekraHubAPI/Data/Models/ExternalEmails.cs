namespace FekraHubAPI.Data.Models
{
    public class ExternalEmails
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public ICollection<MessageSenderExternalEmail> MessageSenderExternalEmails { get; set; }
    }
}
