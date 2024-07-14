using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.EmailSender
{
    public interface IEmailSender
    {

        Task<IActionResult> SendConfirmationEmail(ApplicationUser user);
        Task<IActionResult> SendConfirmationEmailWithPassword(ApplicationUser user , string password);
        Task<IActionResult> SendContractEmail(int studentId, string pdfName);
        Task SendRestPassword(string email, string link);
        Task SendToAdminNewParent(ApplicationUser user);
        Task SendToAllNewEvent();
        Task SendToParentsNewFiles(List<Student> students);
        Task SendToSecretaryNewReportsForStudents();
        Task SendToParentsNewReportsForStudents(List<Student> students);
        Task SendToTeacherReportsForStudentsNotAccepted(int studentId, string teacherId);

    }
}
