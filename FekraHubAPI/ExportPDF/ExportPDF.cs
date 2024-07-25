using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelectPdf;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace FekraHubAPI.ExportReports
{
    public class ExportPDF : IExportPDF
    {
        private readonly IRepository<Report> _reportRepo;
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        public ExportPDF(IRepository<Report> reportRepo, IRepository<SchoolInfo> schoolInfoRepo)
        {
            _schoolInfoRepo = schoolInfoRepo;
            _reportRepo = reportRepo;
        }
        public async Task<string> ExportReport(int reportId)
        {
            HtmlToPdf HtmlToPdf = new HtmlToPdf();
            var reportPage = ReportHtmlPage(reportId);
            PdfDocument Pdf =  HtmlToPdf.ConvertHtmlString(await reportPage);
            byte[] pdfBytes = Pdf.Save();
            Pdf.Close();
            return Convert.ToBase64String(pdfBytes);
        }
        private async Task<string> ReportHtmlPage(int id)
        {
            var logo = (await _schoolInfoRepo.GetRelation()).Select(x => x.LogoBase64).FirstOrDefault();
            var reportData = (await _reportRepo.GetRelation()).Where(x => x.Id == id).Select(x => new
            {
                x.data,
                x.Student.FirstName,
                x.Student.LastName,
                x.Student.Birthday,
                x.Student.Gender,
                courseName = x.Student.Course.Name,
                TeacherFirstName = x.User.FirstName,
                TeacherLastName = x.User.LastName
            }).FirstOrDefault();


            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(reportData.data);

            // بناء سلسلة HTML للجدول
            var htmlTableRows = new StringBuilder();

            foreach (var item in dictionary)
            {
                htmlTableRows.Append($@"
                <tr style=""background-color: #f7f7f7;"">
                    <td style=""border: 1px solid #ddd; padding: 10px 30px; font-size: 20px;"">{item.Key}</td>
                    <td style=""border: 1px solid #ddd; padding: 10px 30px; font-size: 20px;"">{item.Value}</td>
                </tr>");
            }

            string htmlTable = htmlTableRows.ToString();





            string Page = @$"<!DOCTYPE html>
<html lang='en'>

<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Page 2</title>
</head>
<style>
    *{{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: Arial, Helvetica, sans-serif;
            size: A4;
        }}
</style>
<body style=""font-family: Arial, Helvetica, sans-serif; size: A4; margin: 0; padding: 0; box-sizing: border-box;"">
    <div class=""nav-bar"" style=""padding: 40px 20px; padding-top: 50px; display: flex; justify-content: space-between; align-items: center; height: 140px;"">
        <div class=""nav_left"">
            <div style=""display: flex; justify-content: left; align-items: end; line-height: 100px;"">
                <img src=""data:image/png;base64,{logo}"" alt=""logo"" width=""100px"" height=""100px"">
                <h1 class=""company_name"" style=""font-size: 32px; padding-left: 10px; line-height: 100px;"">FekraHub App</h1>
            </div>
            <div style=""display: flex; align-items: end;padding-left:25px;"">
                <div><span style=""font-size: 20px;"">Student's Monthly Report</span></div>
            </div>
        </div>
        <br>
        <div class=""nav_right"" style=""height: 120px; display: flex; flex-direction: column; justify-content: flex-end;"">
            <div style=""margin: 0; padding: 0;font-size: 20px;"">Report Date: <span style=""font-weight: bold;"">07/2024</span></div>
        </div>
    </div>
    <hr>

    <div class='container' style='width: 90%; margin: 0 auto; padding: 20px; margin-top: 20px;'>
        <section class=""info-section"" style=""display: flex; justify-content: space-between; margin-bottom: 20px;"">
            <div class=""left-side"" style=""width: 45%; background-color: #f7f7f7; padding: 20px; border: 1px solid #ddd; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                <h2 style=""margin-top: 0; font-size: 25px; margin-bottom: 20px;"">Student Info</h2>
                <ul style=""list-style: none; padding: 0; margin: 0; padding-left: 20px;"">
                    <li style=""margin-bottom: 10px; font-size: 20px !important;"">First Name: <span style=""font-weight: bold;"">{reportData.FirstName}</span></li>
                    <li style=""margin-bottom: 10px; font-size: 20px !important;"">Last Name: <span style=""font-weight: bold;"">{reportData.LastName}</span></li>
                    <li style=""margin-bottom: 10px; font-size: 20px !important;"">Gender: <span style=""font-weight: bold;"">{reportData.Gender}</span></li>
                </ul>
            </div>
            <div class=""right-side"" style=""width: 45%; background-color: #f7f7f7; padding: 20px; border: 1px solid #ddd; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                <h2 style=""margin-top: 0; font-size: 25px; margin-bottom: 20px;"">Course Info</h2>
                <ul style=""list-style: none; padding: 0; margin: 0; padding-left: 20px;"">
                    <li style=""margin-bottom: 10px; font-size: 20px !important;"">Course Name: <span style=""font-weight: bold;"">{reportData.courseName}</span></li>
                    <li style=""margin-bottom: 10px; font-size: 20px !important;"">Teacher: <span style=""font-weight: bold;"">{reportData.TeacherFirstName} {reportData.TeacherLastName}</span></li>
                </ul>
            </div>
        </section>

        <section class=""table-section"" style=""margin-top: 40px; overflow: hidden; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
            <h2 style=""padding: 20px; "">Report Data</h2>
            <table style=""width: 100%; border-collapse: collapse; border: 1px solid #ddd;"">
                <thead>
                    <tr>
                        <th style=""font-size: 20px; border: 1px solid #ddd; padding: 10px 30px; text-align: left; background-color: #f0f0f0;"">Type</th>
                        <th style=""font-size: 20px; border: 1px solid #ddd; padding: 10px 30px; text-align: left; background-color: #f0f0f0;"">Rating</th>
                    </tr>
                </thead>
                <tbody>
                    {htmlTable}
                </tbody>
            </table>
        </section>
    </div>

    
</body>

</html>
";
    //        < div class=""footer"" style=""position: absolute; width: 100%; padding: 10px; text-align: center;margin-top:50px"">
    //    <p>Report Generated by FekraHub App.</p>
    //</div>
            return Page;
        }
    }
}
