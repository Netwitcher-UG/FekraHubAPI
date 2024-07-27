using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.ExportReports;
using FekraHubAPI.MapModels;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace FekraHubAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IRepository<Report> _reportRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<SchoolInfo> _schoolInfo;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly IExportPDF _exportPDF;
        public ReportsController(IRepository<Report> reportRepo, IRepository<SchoolInfo> schoolInfo,
            IRepository<Student> studentRepo, IEmailSender emailSender,IMapper mapper, IExportPDF exportPDF)
        {
            _reportRepo = reportRepo;
            _schoolInfo = schoolInfo;
            _studentRepo = studentRepo;
            _emailSender = emailSender;
            _mapper = mapper;
            _exportPDF = exportPDF;
        }
        
        [HttpGet("Keys")]
        public async Task<IActionResult> GetReportKeys()
        {
            var keys = (await _schoolInfo.GetRelation()).Select(x => x.StudentsReportsKeys).SingleOrDefault();
            return Ok(keys);
        }
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Report>>> GetAllReports([FromQuery] string? Improved, [FromQuery] int? CourseId, [FromQuery] PaginationParameters paginationParameters)
        {
            IQueryable<Report> query;
            
            if (Improved == null) 
            {
                query = (await _reportRepo.GetRelation()).OrderByDescending(report => report.CreationDate);
            }
            else
            {
                bool? isImproved = null;
                if (Improved.ToLower() == "null")
                {
                    isImproved = null;
                }
                else if (Improved.ToLower() == "true")
                {
                    isImproved = true;
                }
                else if (Improved.ToLower() == "false")
                {
                    isImproved = false;
                }
                else
                {
                    return BadRequest("Invalid value for 'Improved'. Use 'true', 'false', 'null' or leave it empty.");
                }
                query = (await _reportRepo.GetRelation()).Where(sa => sa.Improved == isImproved).OrderByDescending(report => report.CreationDate);
            }
            if (CourseId.HasValue)
            {
                query = query.Where(x => x.Student.CourseID == CourseId);
            }
            if (!query.Any())
            {
                return NotFound("No reports found.");
            }
            var res = await _reportRepo.GetPagedDataAsync(query, paginationParameters);
            var result = new
            {
                res.CurrentPage,
                res.PageSize,
                res.TotalCount,
                res.TotalPages,
                Data = res.Data.Select(x => new
                {
                    x.Id,
                    x.data,
                    x.CreationDate,
                    x.CreationDate.Year,
                    x.CreationDate.Month,
                    TeacherId = x.UserId,
                    TeacherFirstName = x.User.FirstName,
                    TeacherLastName = x.User.LastName,
                    TeacherEmail = x.User.Email,
                    Student = new
                    {
                        x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName,
                        x.Student.Birthday,
                        x.Student.Nationality,
                        x.Student.Note,
                        x.Student.ParentID,
                        course = new { x.Student.CourseID, x.Student.Course.Name }
                    },
                    x.Improved
                })
            };
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = (await _reportRepo.GetRelation()).Where(x => x.Id == id);
            if (report == null)
            {
                return BadRequest($"no report has an ID {id}");
            }
            return Ok(report.Select(x => new
            {
                x.Id,
                x.data,
                x.CreationDate,
                x.CreationDate.Year,
                x.CreationDate.Month,
                TeacherId = x.UserId,
                TeacherFirstName = x.User.FirstName,
                TeacherLastName = x.User.LastName,
                TeacherEmail = x.User.Email,
                Student = new
                {
                    x.Student.Id,
                    x.Student.FirstName,
                    x.Student.LastName,
                    x.Student.Birthday,
                    x.Student.Nationality,
                    x.Student.Note,
                    x.Student.ParentID,
                    course = new { x.Student.CourseID, x.Student.Course.Name }
                },
                x.Improved
            }).FirstOrDefault());
        }
        [HttpGet("Filter")]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports(
            [FromQuery] int? CourseId,
            [FromQuery] string? teacherId,
            [FromQuery] int? studentId,
            [FromQuery] int? reportId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] DateTime? dateTime,
            [FromQuery] string? Improved,
            [FromQuery] PaginationParameters paginationParameters
            )
        {
            IQueryable<Report> query = (await _reportRepo.GetRelation()).OrderByDescending(report => report.CreationDate);
            if (query == null)
            {
                return NotFound("No reports found.");
            }
            if (CourseId.HasValue)
            {
                query = query.Where(x => x.Student.CourseID == CourseId);
            }
            if (!string.IsNullOrEmpty(teacherId))
            {
                query = query.Where(x => x.UserId == teacherId);
            }
            if (studentId.HasValue)
            {
                query = query.Where(x => x.StudentId == studentId);
            }
            if (reportId.HasValue)
            {
                query = query.Where(x => x.Id == reportId);
            }
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(ta => ta.CreationDate >= startDate.Value && ta.CreationDate <= endDate.Value);
            }
            if (year.HasValue)
            {
                query = query.Where(ta => ta.CreationDate.Year == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(ta => ta.CreationDate.Month == month.Value);
            }
            if (dateTime.HasValue)
            {
                query = query.Where(sa => sa.CreationDate == dateTime);
            }
            if (!string.IsNullOrEmpty(Improved))
            {
                bool? isImproved = null;
                if (Improved.ToLower() == "null")
                {
                    isImproved = null;
                }
                else if (Improved.ToLower() == "true")
                {
                    isImproved = true;
                }
                else if (Improved.ToLower() == "false")
                {
                    isImproved = false;
                }
                else
                {
                    return BadRequest("Invalid value for 'Improved'. Use 'true', 'false', 'null' or leave it empty.");
                }
                query = query.Where(sa => sa.Improved == isImproved);
            }
            if (!query.Any())
            {
                return NotFound("No reports found.");
            }
            var res = await _reportRepo.GetPagedDataAsync(query, paginationParameters);
            var result = new
            {
                res.CurrentPage,
                res.PageSize,
                res.TotalCount,
                res.TotalPages,
                Data = res.Data.Select(x => new
                {
                    x.Id,
                    x.data,
                    x.CreationDate,
                    x.CreationDate.Year,
                    x.CreationDate.Month,
                    TeacherId = x.UserId,
                    TeacherFirstName = x.User.FirstName,
                    TeacherLastName = x.User.LastName,
                    TeacherEmail = x.User.Email,
                    Student = new
                    {
                        x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName,
                        x.Student.Birthday,
                        x.Student.Nationality,
                        x.Student.Note,
                        x.Student.ParentID,
                        course = new { x.Student.CourseID, x.Student.Course.Name }
                    },
                    x.Improved
                })
            };

            return Ok(result);

        }

        [HttpPost]
        public async Task<IActionResult> CreateReports(List<Map_Report_Post> map_Report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var DateNow = DateTime.Now;
            var studentIds = new HashSet<int>(map_Report.Select(x => x.StudentId));

            var firstDayOfMonth = new DateTime(DateNow.Year, DateNow.Month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var reports = (await _reportRepo.GetRelation())
                .Where(report => report.CreationDate >= firstDayOfMonth &&
                                 report.CreationDate < firstDayOfNextMonth &&
                                 studentIds.Contains(report.StudentId ?? 0))
                .ToList(); ;

            if (reports.Any())
            {
                return BadRequest($"There is a report for the student with Id: {reports[0].StudentId} on this date {reports[0].CreationDate.Month}/{reports[0].CreationDate.Year}");
            }
            var userId = _reportRepo.GetUserIDFromToken(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            List<Report> AllReports = map_Report.Select(map => new Report
            {
                CreationDate = DateNow,
                StudentId = map.StudentId,
                data = map.data,
                Improved = null,
                UserId = userId
            }).ToList();
            try
            {
                await _reportRepo.ManyAdd(AllReports);
                await _emailSender.SendToSecretaryNewReportsForStudents();
                return Ok(AllReports.Select(x =>  new {
                    x.Id,
                    x.data,
                    x.CreationDate,
                    x.CreationDate.Year,
                    x.CreationDate.Month,
                    x.Improved,
                    x.UserId,
                    x.StudentId
                }).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("AcceptReport")]
        public async Task<IActionResult> AcceptReport(int ReportId)
        {
            var report = await _reportRepo.GetById(ReportId);
            if (report == null)
            {
                return BadRequest("This report not found");
            }
            if (report.Improved == false) 
            {
                return BadRequest("This report needs to updates first");
            }
            report.Improved = true;
            try
            {
                await _reportRepo.Update(report);
                var student = await _studentRepo.GetById(report.StudentId ?? 0);
                await _emailSender.SendToParentsNewReportsForStudents([student]);
                return Ok(new { 
                    report.Id,
                    report.CreationDate,
                    report.CreationDate.Year,
                    report.CreationDate.Month,
                    report.data,
                    report.StudentId,
                    report.UserId,
                    report.Improved
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("UnAcceptReport")]
        public async Task<IActionResult> UnAcceptReport(int ReportId)
        {
            var report = await _reportRepo.GetById(ReportId);
            if (report == null)
            {
                return BadRequest("This report not found");
            }
            report.Improved = false;
            try
            {
                await _reportRepo.Update(report);
                await _emailSender.SendToTeacherReportsForStudentsNotAccepted(report.StudentId ?? 0,report.UserId ?? "");
                return Ok(new {
                    report.Id,
                    report.CreationDate,
                    report.CreationDate.Year,
                    report.CreationDate.Month,
                    report.data,
                    report.StudentId,
                    report.UserId,
                    report.Improved
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        [HttpPatch("AcceptAllReport")]
        public async Task<IActionResult> AcceptAllReport(List<int> ReportIds)
        {
            var AllReports = await _reportRepo.GetRelation();
            var reports = AllReports.Where(x => ReportIds.Contains(x.Id) && x.Improved == null);
            if (!reports.Any())
            {
                return BadRequest("This reports not found");
            }
            try
            {
                await reports.ForEachAsync(report => report.Improved = true);
                await _reportRepo.ManyUpdate(reports);

                List<Student> students = reports.Select(x => x.Student).ToList();
                await _emailSender.SendToParentsNewReportsForStudents(students);
                return Ok(reports.Select(x => new {
                    x.Id,
                    x.CreationDate,
                    x.CreationDate.Year,
                    x.CreationDate.Month,
                    x.data,
                    x.StudentId,
                    x.UserId,
                    x.Improved
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("[action]")]
        public async Task<IActionResult> UpdateReport(int ReportId, string data)
        {
            var report = await _reportRepo.GetById(ReportId);
            if (report == null)
            {
                return BadRequest("This report not found");
            }
            if (report.Improved == true)
            {
                return BadRequest("This report has already been approved");
            }
            report.Improved = null;
            report.data = data;
            try
            {
                await _reportRepo.Update(report);
                await _emailSender.SendToSecretaryUpdateReportsForStudents();
                return Ok(new
                {
                    report.Id,
                    report.CreationDate,
                    report.CreationDate.Year,
                    report.CreationDate.Month,
                    report.data,
                    report.StudentId,
                    report.UserId,
                    report.Improved
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("ExportReport")]
        public async Task<ActionResult<string>> ExportReport(int reportId)
        {
            try
            {
                var report = await _reportRepo.GetById(reportId);
                if (report == null)
                {
                    return BadRequest("no report found");
                }
                if (report.Improved != true)
                {
                    return BadRequest("this report was not approved");
                }
                string reportBase64 = await _exportPDF.ExportReport(reportId);

                return Ok(reportBase64);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }

        }
        //[AllowAnonymous]
        //[HttpPost("test")]
        //public async Task<IActionResult> DownloadReport(int id)
        //{
        //    var x = await _exportPDF.ExportReport(id);

        //    return File(x, "application/pdf", "report.pdf");
        //}
    }
}
