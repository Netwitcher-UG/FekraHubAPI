using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.EmailSender
{
    public interface IEmailSender
    {

        Task<IActionResult> SendConfirmationEmail(ApplicationUser user);
        Task<IActionResult> SendContractEmail(int studentId, string pdfName);
        Task SendToAdminNewParent(ApplicationUser user);
        Task SendToAllNewEvent();
        Task SendToParentsNewFiles();
        Task SendToSecretaryNewReportsForStudents();
        Task SendToParentsNewReportsForStudents();

    }
}
