using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;

using DinkToPdf;
using DinkToPdf.Contracts;
using System.Runtime.ConstrainedExecution;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MimeKit;
using Serilog;

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
        public async Task<string> AttendanceReport(Course course)
        {
            var school = await _schoolInforepo.GetRelationSingle(selector: x => new { x.SchoolName,x.LogoBase64 },
                asNoTracking: true, returnType: QueryReturnType.First);


            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 0, Bottom = 0, Right = 0, Left = 0 },
                DocumentTitle = "Generated PDF"
            };

            var htmlPages = Annualpage(school.SchoolName ?? "",school.LogoBase64, course);
            var objectSettingsList = new List<ObjectSettings>();

            foreach (var htmlPage in htmlPages)
            {
                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = htmlPage,
                    WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true, LoadImages = true },
                };
                objectSettingsList.Add(objectSettings);
            }

            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings
            };

            foreach (var obj in objectSettingsList)
            {
                document.Objects.Add(obj);
            }

            byte[] pdfFile = _converter.Convert(document);
            return Convert.ToBase64String(pdfFile);
        }
        //        var startDate = "";
        //        var endDate = "";
        //                for (int i = 0; i<currentWorkDays.Count; i++)
        //                {
        //                    if(i == 0)
        //                    {
        //                        startDate = currentWorkDays[i].ToString("dd.MM.yyyy");
        //    }else if (i == currentWorkDays.Count - 1)
        //                    {
        //                        endDate = currentWorkDays[i].ToString("dd.MM.yyyy");
        //}
        //var cssClass = (i % 2 == 0) ? "gray" : "";
        //header += $@"<th class='{cssClass}'>{currentWorkDays[i].ToString("dd.MM")}</th>";
        //                }
        //                var noun = $@"
        //                    <div style=""width: 100%;text-align: center;margin-top: 50px;"">
        //                           <h3>{startDate}/{endDate} - {course.Name}</h3>
        //                    </div>
        //                    ";

        private List<string> Annualpage(string schoolName,string logo, Course course)
        {
            
            var teacher = course.Teacher.FirstOrDefault();
            var teacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "";
            var roomName = course.Room != null ? course.Room.Name : "";
            var students = course.Student.ToList();
            var schedule = course.CourseSchedule.ToList() ?? new List<CourseSchedule>();

            var courseStartDate = course.StartDate.Date;
            var courseEndDate = course.EndDate.Date;
            var workDays = new List<DateTime>();

            // إنشاء قائمة الأيام الدراسية
            foreach (var day in schedule)
            {
                var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day.DayOfWeek, true);
                var currentDay = courseStartDate;
                while (currentDay.DayOfWeek != dayOfWeek)
                {
                    currentDay = currentDay.AddDays(1);
                }

                while (currentDay <= courseEndDate)
                {
                    workDays.Add(currentDay);
                    currentDay = currentDay.AddDays(7);
                }
            }

            workDays.Sort();

            int studentsPerPage = 15; // عدد الطلاب في كل صفحة
            int daysPerPage = 20; // عدد الأعمدة في كل صفحة
            var totalStudentPages = (int)Math.Ceiling((double)students.Count / studentsPerPage); // عدد الصفحات للطلاب
            var totalDayPages = (int)Math.Ceiling((double)workDays.Count / daysPerPage); // عدد الصفحات للأعمدة

            var htmlPages = new List<string>();

            // القالب الذي يتكرر في كل صفحة (الهيدر والفوتر)
            string template = @"
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
            margin: 0;
            box-sizing: border-box;
            font-size: 14px;
        }
        h3 {
            font-size: 16px !important;
        }
        
        .container{
            width: 1400px; 
            height: 990px;
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
      </style>" + $@"
    </head>
    <body >
        <div class='container'>
        <div style='width: 1395px; height: 100px;'>
            <table style='width:100%; padding-top:20px;'>
                <tr>
                    <td style='width:10%;text-align:right;padding: 0;height: 80px;'>
                        <img alt='logo' width='80px' style='padding:0;opacity: 1;' src='data:image/png;base64,{logo}'>
                    </td>
                    <td style='width:10%;text-align:left;padding: 0;'>
                        <h1 style='font-size: 28px;color: rgba(0, 0, 0, 0.5);'>{schoolName}</h1>
                    </td>
                    <td style='width:80%;'></td>
                </tr>
            </table>
            <div style='position: relative;width: 100%;'>
                <div style='border-top:3px solid rgba(0, 0, 0, 0.082) ;width: 100%;'>
                </div>
            </div>
        </div>

        <div style='width: 1370px; height: 670px;min-height: 670px !important;padding: 20px;padding-top:50px;'>

            <table style=""width: 100%; border-collapse: collapse;"">
                <tr>
                    <td class=""tdWithPadding"">Anwesenheitsliste</td>
                    <td class=""tdWithPadding"">Klasse :  {course.Name}  / نحضيري    </td>
                    <td class=""tdWithPadding"">Lehrerin : {teacherName}</td>
                    <td class=""tdWithPadding"">Seminarraum : {roomName}</td>
                </tr>
            </table>";

            string footer = @"
        <div style='width: 100%; height: 150px;border-top: 1px solid rgba(0, 0, 0, 0.11);background-color: white;z-index: 1000;position: fixed;bottom: 0;left: 0;'>
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
                                <h1 style='padding: 20px;font-size: 18px;'>مدرسة فكرة ... <br>الطريق الأفضل لتعلم اللغة العربية</h1>
                            </div>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </div>
</body>
</html>";

            // تكرار لكل مجموعة من 20 يومًا (أعمدة)
            for (int dayPage = 0; dayPage < totalDayPages; dayPage++)
            {
                var currentWorkDays = workDays.Skip(dayPage * daysPerPage).Take(daysPerPage).ToList();

                // تكرار لكل مجموعة من الطلاب (15 طالب)
                for (int group = 0; group < totalStudentPages; group++)
                {
                    var studentGroup = students.Skip(group * studentsPerPage).Take(studentsPerPage).ToList();

                    // إنشاء رأس الجدول مع الأعمدة
                    var header = $@"
                <thead>
                <tr>
                    <th rowspan=""2"" colspan=""2"" style=""font-size: 20px;"">Name</th>
                </tr>
                <tr>";

                    for (int i = 0; i < currentWorkDays.Count; i++)
                    {
                        var cssClass = (i % 2 == 0) ? "gray" : "";
                        header += $@"<th class='{cssClass}'>{currentWorkDays[i].ToString("dd.MM")}</th>";
                    }

                    header += "</tr></thead>";

                    // إنشاء صفوف الطلاب
                    var row = "";
                    for (var i = 0; i < studentGroup.Count; i++)
                    {
                        var studentName = studentGroup[i].FirstName + " " + studentGroup[i].LastName;
                        var studentAttendance = studentGroup[i].StudentAttendance;

                        row += $@"
                <tr>
                <td class='gray' style='width:10px;'>{(group * studentsPerPage) + i + 1}</td>
                <td class='header-col'>{studentName}</td>";

                        // إنشاء خلايا الحضور لكل يوم
                        for (int j = 0; j < currentWorkDays.Count; j++)
                        {
                            var currentDay = currentWorkDays[j];
                            var attendanceStatus = studentAttendance?.FirstOrDefault(a => a.date.Date == currentDay.Date)?.AttendanceStatus?.Title ?? "";
                            var cssClass = (j % 2 == 0) ? "gray" : "";

                            string attendanceIcon;
                            if (attendanceStatus == "Present")
                            {
                                attendanceIcon = "<span style='color: green;'>&#10004;</span>";
                            }
                            else if (attendanceStatus == "Absent")
                            {
                                attendanceIcon = "<span style='color: red;'>&#10008;</span>";
                            }
                            else
                            {
                                attendanceIcon = "";
                            }

                            row += $@"<td class='{cssClass}'>{attendanceIcon}</td>";
                        }

                        row += "</tr>";
                    }

                    // إنشاء الصفحة HTML مع القالب والتفاصيل
                    var htmlPage = template + $@"
                <div style='width: 100%;text-align: center;margin-top: 50px;'>
                    <h3>{currentWorkDays.First().ToString("dd.MM.yyyy")} - {currentWorkDays.Last().ToString("dd.MM.yyyy")} - {course.Name}</h3>
                </div>
                <table class=""AttendanceTable"" style=""z-index: 1;margin-top:30px;"">
                   {header}
                    <tbody>
                        {row}
                    </tbody>
                </table>
            " + footer;

                    htmlPages.Add(htmlPage); // إضافة الصفحة إلى القائمة
                }
            }

            return htmlPages;
        }


        public int GetTableColumnCount(Course course)
        {
            var courseStartDate = course.StartDate.Date;
            var courseEndDate = course.EndDate.Date;
            var schedule = course.CourseSchedule.ToList() ?? new List<CourseSchedule>();
            var workDays = new List<DateTime>();

            foreach (var day in schedule)
            {
                var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day.DayOfWeek, true);

                var currentDay = courseStartDate;
                while (currentDay.DayOfWeek != dayOfWeek)
                {
                    currentDay = currentDay.AddDays(1);
                }

                while (currentDay <= courseEndDate)
                {
                    workDays.Add(currentDay);
                    currentDay = currentDay.AddDays(7);
                }
            }
            workDays.Sort();

            int totalColumnsCount = workDays.Count;
            return totalColumnsCount;
        }


        public async Task<string> MonthlyAttendanceReport(Course course,DateTime date)
        {
            var school = await _schoolInforepo.GetRelationSingle(selector: x => new { x.SchoolName , x.LogoBase64 },
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
                HtmlContent = Monthlypage(school.SchoolName??"",school.LogoBase64,course,date),
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
        private string Monthlypage(string schoolName, string logo, Course course,DateTime date)
        {
            var teacher = course.Teacher.FirstOrDefault();
            var teacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "";
            var roomName = course.Room != null ? course.Room.Name : "";
            var row = @"";
            var students = course.Student.ToList();
            var schedule = course.CourseSchedule.ToList() ?? new List<CourseSchedule>();

            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var workDays = new List<DateTime>();

            foreach (var day in schedule)
            {
                var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day.DayOfWeek, true);

                var currentDay = monthStart;
                while (currentDay.DayOfWeek != dayOfWeek)
                {
                    currentDay = currentDay.AddDays(1);
                }

                while (currentDay <= monthEnd)
                {
                    workDays.Add(currentDay);
                    currentDay = currentDay.AddDays(7); 
                }
            }
            workDays.Sort();
            var attendanceList = course.Student.Select(x => x.StudentAttendance).ToList();

            var header = $@"
                <thead>
                <tr>
                    <th rowspan=""2"" colspan=""2"" style=""font-size: 20px;"">Name</th>
                    
                </tr>
            <tr>";

            for (int i = 0; i < workDays.Count; i++)
            {
                var cssClass = (i % 2 == 0) ? "gray" : "";
                header += $@"<th class='{cssClass}'>{workDays[i].ToString("dd.MM")}</th>";
            }

            header += "</tr></thead>";

            for (var i = 0; i < 30; i++)
            {
                var studentName = i < students.Count ? students[i].FirstName + " " + students[i].LastName : "";
                var studentAttendance = i < students.Count ? students[i].StudentAttendance : null;

                row += $@"
                    <tr>
                    <td class='gray' style='width:10px;'>{i + 1}</td>
                <td class='header-col'>{studentName}</td>";

                for (int j = 0; j < workDays.Count; j++)
                {
                    var currentDay = workDays[j];
                    var attendanceStatus = studentAttendance?.FirstOrDefault(a => a.date == currentDay)?.AttendanceStatus?.Title ?? ""; // استخراج حالة الحضور
                    var cssClass = (j % 2 == 0) ? "gray" : "";

                    string attendanceIcon;
                    if (attendanceStatus == "Present") 
                    {
                        attendanceIcon = "<span style='color: green;'>&#10004;</span>"; 
                    }
                    else if (attendanceStatus == "Absent") 
                    {
                        attendanceIcon = "<span style='color: red;'>&#10008;</span>";
                    }
                    else
                    {
                        attendanceIcon = ""; 
                    }

                    row += $@"<td class='{cssClass}'>{attendanceIcon}</td>";
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
                    <img alt='logo' width='80px' style='padding:0;opacity: 1;' src='data:image/png;base64,{logo}'>
                </td>
                <td style='width:10%;text-align:left;padding: 0;'>
                    <h1 style='font-size: 28px;color: rgba(0, 0, 0, 0.5);'>{schoolName}</h1>
                </td>
                <td style='width:80%;'></td>
            </tr>
        </table>
        <div style='position: relative;width: 100%;'>
            <div style='border-top:3px solid rgba(0, 0, 0, 0.082) ;width: 100%;'>
            </div>
        </div>
    </div>


    <div style='width: 955px; height: 1070px;padding: 20px;padding-top:50px;'>

        <table style=""width: 100%; border-collapse: collapse;"" >
            <tr>
                <td class=""tdWithPadding"">Anwesenheitsliste</td>
                <td class=""tdWithPadding"">Klasse :  {course.Name}  / نحضيري    </td>
                <td class=""tdWithPadding"">Lehrerin : {teacherName}</td>
                <td class=""tdWithPadding"">Seminarraum : {roomName}</td>
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


    <div style='width: 100%; height: 150px;border-top: 1px solid rgba(0, 0, 0, 0.11);background-color: white;z-index: 1000;position: fixed;bottom: 0;left: 0;'>
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
