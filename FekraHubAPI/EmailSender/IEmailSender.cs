using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.EmailSender
{
    public interface IEmailSender
    {

        Task SendConfirmationEmail(ApplicationUser user,string? yourEmail=null);
        Task SendConfirmationEmailWithPassword(ApplicationUser user , string password, string? yourEmail=null);
        Task SendContractEmail(int studentId, string pdfName, string? yourEmail = null);
        Task SendRestPassword(string email, string link, string? yourEmail = null);
        Task SendToAdminNewParent(ApplicationUser user, string? yourEmail = null);
        Task SendToAdminNewStudent(Student student, string? yourEmail = null);
        Task SendToAllNewEvent(List<int?> corsesId, string? yourEmail = null);
        Task SendToParentsNewFiles(int coursId, string? yourEmail = null);
        Task SendToSecretaryNewReportsForStudents( string? yourEmail = null);
        Task SendToSecretaryUpdateReportsForStudents(string? yourEmail = null);
        Task SendToParentsNewReportsForStudents(List<Student> students, string? yourEmail = null);
        Task SendToTeacherReportsForStudentsNotAccepted(int studentId, string teacherId, string? yourEmail = null);

        Task SendConfirmationEmailFromExcel(ApplicationUser user, string password, string? yourEmail = null);
        Task AcceptStudent(ApplicationUser parent,Student student,byte[] pdf, string? yourEmail = null);
        Task RejectStudentForParent(ApplicationUser user, string reason, string? yourEmail = null);

    }
}
