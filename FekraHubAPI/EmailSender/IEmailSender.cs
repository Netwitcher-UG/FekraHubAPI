using FekraHubAPI.Data.Models;

namespace FekraHubAPI.EmailSender
{
    public interface IEmailSender
    {
        
        Task SendConfirmationEmail(ApplicationUser user);

    }
}
