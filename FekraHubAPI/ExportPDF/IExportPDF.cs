using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.ExportReports
{
    public interface IExportPDF
    {
        Task<string> ExportReport(int reportId);
    }
}
