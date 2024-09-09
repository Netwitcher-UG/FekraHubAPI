using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.EmailSender
{
    public interface IEmailSender
    {

        Task SendConfirmationEmail(ApplicationUser user);
        Task SendConfirmationEmailWithPassword(ApplicationUser user , string password);
        Task SendContractEmail(int studentId, string pdfName);
        Task SendRestPassword(string email, string link);
        Task SendToAdminNewParent(ApplicationUser user);
        Task SendToAdminNewStudent(Student student);
        Task SendToAllNewEvent(List<int?> corsesId);
        Task SendToParentsNewFiles(int coursId);
        Task SendToSecretaryNewReportsForStudents();
        Task SendToSecretaryUpdateReportsForStudents();
        Task SendToParentsNewReportsForStudents(List<Student> students);
        Task SendToTeacherReportsForStudentsNotAccepted(int studentId, string teacherId);

    }
}
