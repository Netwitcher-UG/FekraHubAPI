namespace FekraHubAPI.Data.Models
{
    public class NotificationUser
    {
        public int NotificationId { get; set; }
        public Notifications Notifications { get; set; }

        public string UserId { get; set; }
        public ApplicationUser ApplicationUsers { get; set; }
        public bool Read { get; set; } = false;
    }
}
