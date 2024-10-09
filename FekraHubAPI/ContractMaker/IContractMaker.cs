using FekraHubAPI.Data.Models;

namespace FekraHubAPI.ContractMaker
{
    public interface IContractMaker
    {
        Task ConverterHtmlToPdf(Student student);
        Task<byte[]> GetContractPdf(int studentId);
        Task<string> ContractHtml(Student student);
        Task<string> MonthlyAttendanceReport(Course course, DateTime date);
        Task<string> AttendanceReport(Course courses);
    }
}
