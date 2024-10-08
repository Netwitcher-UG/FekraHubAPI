using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;

using DinkToPdf;
using DinkToPdf.Contracts;
using System.Runtime.ConstrainedExecution;
using System;

namespace FekraHubAPI.ContractMaker
{
    public class ContractMaker : IContractMaker
    {
        private readonly IRepository<StudentContract> _repo;
        private readonly IRepository<ApplicationUser> _Usersrepo;
        private readonly IRepository<Student> _studentrepo;
        private readonly IRepository<SchoolInfo> _schoolInforepo;
        private readonly IRepository<ContractPage> _contractPagesrepo;
        private readonly IConverter _converter;
        public ContractMaker(IRepository<StudentContract> repo, IRepository<Student> studentrepo,
            IRepository<ApplicationUser> Usersrepo, IRepository<SchoolInfo> schoolInforepo, IConverter converter, IRepository<ContractPage> contractPagesrepo)
        {
            _repo = repo;
            _studentrepo = studentrepo;
            _Usersrepo = Usersrepo;
            _schoolInforepo = schoolInforepo;
            _converter = converter;
            _contractPagesrepo = contractPagesrepo;
        }
        private async Task<byte[]> PdfFile(Student student)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 0, Bottom = 0, Right = 0, Left = 0 },
                DocumentTitle = "Generated PDF"
            };
            var objectSettings1 = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = (await ContractHtmlPage(student))[0],
                WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true, LoadImages = true },
            };
            var objectSettings2 = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = (await ContractHtmlPage(student))[1],
                WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true, LoadImages = true },
            };
            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings1, objectSettings2 }
            };
            byte[] pdfFile = _converter.Convert(document);
            return pdfFile;
        }
        public async Task ConverterHtmlToPdf(Student student)
        {
            try
            {
                byte[] pdfFile = await PdfFile(student);
                StudentContract studentContract = new()
                {
                    StudentID = student.Id,
                    File = pdfFile,
                    CreationDate = DateTime.Now
                };
                await _repo.Add(studentContract);
            }
            catch (Exception ex)
            {
                if (await _studentrepo.IDExists(student.Id))
                {
                    await _studentrepo.Delete(student.Id);
                }
                if (await _repo.DataExist(x => x.StudentID == student.Id))
                {
                    await _repo.Delete(student.Id);
                }
            }


        }
        public async Task<byte[]> GetContractPdf(int studentId)
        {
            var AllContracts = await _repo.GetAll();
            var contract = AllContracts.Where(c => c.StudentID == studentId).First();
            return contract.File;
        }
        public async Task<string> ContractHtml(Student student)
        {
            byte[] x = await PdfFile(student);
            return Convert.ToBase64String(x);
        }
        private async Task<List<string>> ContractHtmlPage(Student student)
        {
            var schoolInfoLogo = await _schoolInforepo.GetRelationSingle(
                selector: x => x.LogoBase64,
                returnType: QueryReturnType.Single,
                asNoTracking: true);
            if (schoolInfoLogo == null)
            {
                return new List<string>();
            }
            var parent = await _Usersrepo.GetUser(student.ParentID ?? "");
            if (parent == null)
            {
                return new List<string>();
            }
            List<string> contractPages = (await _contractPagesrepo.GetAll()).Select(x => x.ConPage).ToList();

            contractPages[0] = contractPages[0]
                .Replace("{student.FirstName}", student.FirstName ?? "")
                .Replace("{student.LastName}", student.LastName ?? "")
                .Replace("{student.Birthday.Date.ToString('yyyy-MM-dd')}", student.Birthday.Date.ToString("yyyy-MM-dd"))
                .Replace("{student.Nationality}", student.Nationality ?? "")
                .Replace("{parent.FirstName}", parent.FirstName ?? "")
                .Replace("{parent.LastName}", parent.LastName ?? "")
                .Replace("{parent.Street}", parent.Street ?? "")
                .Replace("{parent.StreetNr}", parent.StreetNr ?? "")
                .Replace("{parent.ZipCode}", parent.ZipCode ?? "")
                .Replace("{parent.EmergencyPhoneNumber}", parent.EmergencyPhoneNumber ?? "")
                .Replace("{parent.PhoneNumber}", parent.PhoneNumber ?? "")
                .Replace("{parent.Email}", parent.Email ?? "");
            for (var i = 0; i < contractPages.Count(); i++)
            {
                contractPages[i] = contractPages[i].Replace("{fekrahublogo}", schoolInfoLogo ?? "");
            }
            return contractPages;
        }


        public async Task<string> AttendanceReport(Course course,DateTime date)
        {
            var schoolName = await _schoolInforepo.GetRelationSingle(selector: x => x.SchoolName,
                asNoTracking:true,returnType:QueryReturnType.First);
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 0, Bottom = 0, Right = 0, Left = 0 },
                DocumentTitle = "Generated PDF"
            };
            var objectSettings1 = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = page1(schoolName??"",course,date),
                WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true, LoadImages = true },
            };
            
            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings1 }
            };
            byte[] pdfFile = _converter.Convert(document);
            return Convert.ToBase64String( pdfFile);

        }
        private string page1(string schoolName,Course course,DateTime date)
        {
            var teacher = course.Teacher.FirstOrDefault();
            var row = @"";
            var students = course.Student.ToList();
            var schedule = course.CourseSchedule.ToList();

            // استخراج تواريخ أيام الدوام بناءً على الشهر المحدد
            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var workDays = new List<DateTime>();

            // إيجاد جميع الأيام في الشهر المحدد التي توافق أيام الدوام في الجدول
            foreach (var day in schedule)
            {
                // day.Name يحتوي على اسم اليوم مثل "Saturday" أو "Sunday"
                var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day.DayOfWeek, true);

                // إيجاد أول يوم يوافق اليوم المحدد في الشهر
                var currentDay = monthStart;
                while (currentDay.DayOfWeek != dayOfWeek)
                {
                    currentDay = currentDay.AddDays(1);
                }

                // إضافة كل الأيام التي توافق هذا اليوم (كل أسبوع) حتى نهاية الشهر
                while (currentDay <= monthEnd)
                {
                    workDays.Add(currentDay);
                    currentDay = currentDay.AddDays(7); // زيادة أسبوع كامل
                }
            }
            workDays.Sort();
            // تجهيز جدول الحضور
            var attendanceList = course.Student.Select(x => x.StudentAttendance).ToList();

            // إنشاء رأس الجدول لتواريخ الدوام
            var header = $@"
                <thead>
                <tr>
                    <th rowspan=""2"" colspan=""2"" style=""font-size: 20px;"">Name</th>
                    <th colspan=""{workDays.Count}"">Bücher</th>
                </tr>
            <tr>";

            // إضافة التواريخ إلى رأس الجدول
            for (int i = 0; i < workDays.Count; i++)
            {
                var cssClass = (i % 2 == 0) ? "gray" : "";
                header += $@"<th class='{cssClass}'>{workDays[i].ToString("dd.MM")}</th>";
            }

            header += "</tr></thead>";

            // إنشاء الصفوف لكل طالب
            for (var i = 0; i < 30; i++)
            {
                var studentName = i < students.Count ? students[i].FirstName + " " + students[i].LastName : "";
                var studentAttendance = i < students.Count ? students[i].StudentAttendance : null;

                row += $@"
                    <tr>
                    <td>{i + 1}</td>
                <td class='header-col'>{studentName}</td>";

                // نضيف حالة الحضور لكل يوم دوام
                for (int j = 0; j < workDays.Count; j++)
                {
                    var currentDay = workDays[j];
                    var attendanceStatus = studentAttendance?.FirstOrDefault(a => a.date == currentDay)?.AttendanceStatus.Title ?? ""; // استخراج حالة الحضور
                    var cssClass = (j % 2 == 0) ? "gray" : ""; // تغيير اللون بناءً على رقم اليوم

                    row += $@"<td class='{cssClass}'>{attendanceStatus}</td>";
                }

                row += "</tr>";
            }
            var x = @"
              <!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Document</title>
  <style>
     
    body{
        font-family: Arial, Helvetica, sans-serif;
        padding: 0 !important;
        padding: 0;
        margin: 0;
        box-sizing: border-box;
        font-size: 14px;
    }
    h3 {
        font-size: 16px !important;
    }
    
    .container{
        width: 995px;
         height: 1400px;
         margin: 0 auto;
         padding: 0 !important;
        text-align: left;
         overflow: hidden;
    }
   
    h5{
        margin: 20px 0;
    }
    .tdWithPadding{
        border: 1px solid black;
        padding: 5px;
    }
    .td1{
        border: 1px solid black;
        padding: 0 !important;
        border-bottom: none;
        border-right: none;
    }
    .AttendanceTable {
        border-collapse: collapse;
        width: 100%;
    }
    .AttendanceTable th,
    .AttendanceTable td {
        border: 1px solid black;
        padding: 5px;
        text-align: center;
    }
    
    .header-col {
        text-align: left !important;
        padding-left: 20px !important;
        padding-right: 20px !important;
        white-space: nowrap;
        overflow: hidden;  
        text-overflow: ellipsis; 
    }
    .gray {
        background-color: #d3d3d3;
    }
    .number{
        width: 5px;
    }
</style>
" + $@"
</head>
<body >
    <div class='container'>
    <div style='width: 990px; height: 100px;'>
        <table style='width:100%; padding-top:20px;'>
            <tr>
                <td style='width:10%;text-align:right;padding: 0;height: 80px;'>
                    <img alt='logo' width='80px' style='padding:0;opacity: 0.8;' src='https://devapi.fekrahub.app/api/SchoolInfo/SchoolLogo1'>
                </td>
                <td style='width:10%;text-align:left;padding: 0;'>
                    <h1 style='font-size: 28px;color: rgba(0, 0, 0, 0.5);'>{schoolName}</h1>
                </td>
                <td style='width:80%;'></td>
            </tr>
        </table>
        <div style='position: relative;width: 100%;'>
            <div style='border-top:3px solid rgba(0, 0, 0, 0.082) ;position: absolute;top: -31px;left: 0;width: 100%;'>
            </div>
        </div>
    </div>


    <div style='width: 955px; height: 1070px;padding: 20px;padding-top:50px;'>

        <table style=""width: 100%; border-collapse: collapse;"" >
            <tr>
                <td class=""tdWithPadding"">Anwesenheitsliste</td>
                <td class=""tdWithPadding"">Klasse :  {course.Name}  / نحضيري    </td>
                <td class=""tdWithPadding"">Lehrerin : {teacher.FirstName} {teacher.LastName}</td>
                <td class=""tdWithPadding"">Seminarraum : {course.Room.Name}</td>
            </tr>
        </table>

        <div style=""width: 100%;text-align: center;margin-top: 50px;"">
            <h3>{date.Month}.{date.Year} - {course.Name}</h3>
        </div>

        
        <table class=""AttendanceTable"" style=""z-index: 1;"">
           {header}
            <tbody>
                {row}
               </tbody>
              </table>


    </div>


    <div style='width: 100%; height: 150px;border-top: 1px solid rgba(0, 0, 0, 0.11);background-color: white;z-index: 1000;'>
        <table style='width:100%;'>
            <tr>
                <td style='width:10%;text-align:right;padding: 20px;'>
                    <div style='text-align:left;'>
                        <h4>
                            Telefon : 01794169927<br></br>
                            Email : Admin@fekraschule.de<br></br>
                            Adresse : Kleiststraße 23-26, 10787 Berlin
                        </h4>
                    </div>
                </td>
                <td style='width:10%;text-align:left;padding: 0;'>
                    <div dir='rtl' style='text-align: right;padding-top: 10px;'>
                        <div style='color: white;background-color: black;width: 80%;margin-right: 5px;'>
                            <h1 style='padding: 20px;font-size: 18px;'>مدرسة فكرة ... <br>الطريق الأفضل لتعلم اللغة
                                العربية</h1>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</div>
</body>
</html>
                    ";
            return x;
        }
       
    }

}
