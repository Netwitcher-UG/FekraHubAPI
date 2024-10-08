﻿using DinkToPdf;
using DinkToPdf.Contracts;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;


namespace FekraHubAPI.ExportReports
{
    public class ExportPDF : IExportPDF
    {
        private readonly IRepository<Report> _reportRepo;
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly IConverter _converter;
        public ExportPDF(IRepository<Report> reportRepo, IRepository<SchoolInfo> schoolInfoRepo , IConverter converter)
        {
            _schoolInfoRepo = schoolInfoRepo;
            _reportRepo = reportRepo;
            _converter = converter;
        }
        public async Task<string> ExportReport(int reportId)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 0, Bottom = 6, Right = 0, Left = 0 },
                DocumentTitle = "Generated PDF"
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = await ReportHtmlPage(reportId),
                
                FooterSettings = {
                    FontSize = 12, 
                    Line = true, 
                    Center = @"Report Generated by FekraHub App"
                }
            };
            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            byte[] x = _converter.Convert(document);
            return Convert.ToBase64String(x); 
        }
        private async Task<string> ReportHtmlPage(int id)
        {
            var logo = await _schoolInfoRepo.GetRelationSingle(
                selector: x => x.LogoBase64,
                returnType: QueryReturnType.Single,
                asNoTracking: true);
            var reportData = await _reportRepo.GetRelationSingle(
                where: x => x.Id == id,
                include:x=>x.Include(s=>s.Student).ThenInclude(c=>c.Course).Include(u=>u.User),
                selector: x => new
                {
                    x.data,
                    x.Student.FirstName,
                    x.Student.LastName,
                    x.Student.Birthday,
                    x.Student.Gender,
                    courseName = x.Student.Course.Name,
                    TeacherFirstName = x.User.FirstName,
                    TeacherLastName = x.User.LastName
                },
                returnType:QueryReturnType.SingleOrDefault,
                asNoTracking: true);

            
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(reportData.data);

            var htmlTableRows = new StringBuilder();

            foreach (var item in dictionary)
            {
                htmlTableRows.Append($@"
                <tr style='background-color: #f7f7f7;'>
                    <td style='border: 1px solid #ddd; padding: 10px 30px; font-size: 20px;'>{item.Value}</td>
                    <td style='border: 1px solid #ddd; padding: 10px 30px; font-size: 20px;width:60%'>{item.Key}</td>
                    
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
                                    *{{margin: 0; padding: 0; box-sizing: border-box;
                                    }}
                                </style>
                               
<body style='font-family: Arial, Helvetica, sans-serif; size: A4; margin: 0; padding: 0; box-sizing: border-box;'>
  
    <table style='width:100%;'>
        <tr >
           <td style='width:10%;text-align:right;padding: 0;'>
               <img alt='logo' width='100px' height='100px' src='data:image/png;base64,{logo}' >
           </td>
           <td style='width:10%;text-align:left;padding: 0;'>
               <h1 style='font-size: 28px;line-height:100px;'>FekraHubApp</h1>
           </td>
           <td style='width:60%;'></td>
           <td style='width:20%;text-align:right;padding: 0;'>
               <div style='text-align:left;padding-left: 40px;width: 250px;'>
                   <div><span style='font-size: 18px;'>Student's Monthly Report</span></div>
                   <div style='font-size: 18px;margin-top:3px'>Report Date: <span style='font-weight: bold;'>07/2024</span></div>
               </div>
           </td>
       </tr>
   </table>
   
<hr>
    <div class='container' style='width: 100%; margin: 0 auto; '>
        <table style='width:100%;text-align:left;'>
                <tr style='width:100%;'>
<td style='width:50%;padding:20px;'>
            <div class='left-side' style=' min-height:180px; background-color: #f7f7f7; padding: 20px; border: 1px solid #ddd; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                <h2 style='margin-top: 0; font-size: 25px; margin-bottom: 20px;'>Student Info</h2>
                <ul style='list-style: none; padding: 0; margin: 0; padding-left: 20px;'>
                    <li style='margin-bottom: 10px; font-size: 20px !important;'>First Name: <span style='font-weight: bold;'>{reportData.FirstName}</span></li>
                    <li style='margin-bottom: 10px; font-size: 20px !important;'>Last Name: <span style='font-weight: bold;'>{reportData.LastName}</span></li>
                    <li style='margin-bottom: 10px; font-size: 20px !important;'>Gender: <span style='font-weight: bold;'>{reportData.Gender}</span></li>
                </ul>
            </div>
</td>
<td style='width:50%;padding:20px;'>
            <div class='right-side' style='min-height:180px; background-color: #f7f7f7; padding: 20px; border: 1px solid #ddd; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                <h2 style='margin-top: 0; font-size: 25px; margin-bottom: 20px;'>Course Info</h2>
                <ul style='list-style: none; padding: 0; margin: 0; padding-left: 20px;'>
                    <li style='margin-bottom: 10px; font-size: 20px !important;'>Course Name: <span style='font-weight: bold;'>{reportData.courseName}</span></li>
                    <li style='margin-bottom: 10px; font-size: 20px !important;'>Teacher: <span style='font-weight: bold;'>{reportData.TeacherFirstName} {reportData.TeacherLastName}</span></li>
                </ul>
            </div>
<td>
</tr>
</table>
        </div>

        <section class='table-section' style='margin:20px; margin-top: 40px; overflow: hidden; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
            <h2 style='padding: 20px; '>Report Data</h2>
            <table style='width: 100%; border-collapse: collapse; border: 1px solid #ddd;text-align: center;'>
                <thead>
                    <tr>
                        <th style='font-size: 20px; border: 1px solid #ddd; padding: 10px 30px; text-align: center; background-color: #f0f0f0;'>Rating</th>
                        <th style='font-size: 20px; border: 1px solid #ddd; padding: 10px 30px; text-align: center; background-color: #f0f0f0;width:60%'>Type</th>
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
            return Page;
        }
    }
}
  