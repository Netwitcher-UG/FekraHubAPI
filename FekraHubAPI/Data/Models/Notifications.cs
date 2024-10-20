namespace FekraHubAPI.Data.Models
{
    public class Notifications
    {
        public int Id { get; set; }
        public string Notification { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        
        public ICollection<NotificationUser> NotificationUsers { get; set; }
    }
}
