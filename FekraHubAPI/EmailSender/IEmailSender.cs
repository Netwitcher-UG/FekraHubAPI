using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.EmailSender
{
    public interface IEmailSender
    {

        Task<IActionResult> SendConfirmationEmail(ApplicationUser user);

    }
}
