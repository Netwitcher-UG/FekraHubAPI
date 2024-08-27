using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.ExportReports;
using FekraHubAPI.MapModels;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;



namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IRepository<Report> _reportRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<StudentsReportsKey> _studentsReportsKeyInfo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly IExportPDF _exportPDF;
        private readonly UserManager<ApplicationUser> _Users;
        private readonly ILogger<ReportsController> _logger;
        public ReportsController(IRepository<Report> reportRepo, IRepository<StudentsReportsKey> studentsReportsKeyInfo,
            IRepository<Student> studentRepo, IEmailSender emailSender,IMapper mapper, IExportPDF exportPDF
            , UserManager<ApplicationUser> Users, IRepository<Course> courseRepo, ILogger<ReportsController> logger)
        {
            _reportRepo = reportRepo;
            _studentsReportsKeyInfo = studentsReportsKeyInfo;
            _studentRepo = studentRepo;
            _emailSender = emailSender;
            _mapper = mapper;
            _exportPDF = exportPDF;
            _Users = Users;
            _courseRepo = courseRepo;
            _logger = logger;
        }
        [Authorize(Policy = "InsertUpdateStudentsReports")]
        [HttpGet("Keys")]
        public async Task<IActionResult> GetReportKeys()
        {
            try
            {
                var keys = await _studentsReportsKeyInfo.GetRelation<string>(null, null, x => x.Keys);
                return Ok(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "GetStudentsReports")]
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Report>>> GetAllReports(
            [FromQuery] string? Improved,
            [FromQuery] int? CourseId, 
            [FromQuery] PaginationParameters paginationParameters)
        {
            try
            {
                bool? isImproved = null;
                if (Improved != null)
                {
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
                }
                var userId = _reportRepo.GetUserIDFromToken(User);
                var Teacher = await _reportRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    if (CourseId != null)
                    {
                        var course = (await _courseRepo.GetRelation<Course>(x => x.Id == CourseId)).FirstOrDefault();
                        if (course != null)
                        {
                            var teacherIds = course.Teacher.Select(x => x.Id);
                            if (!teacherIds.Contains(userId))
                            {
                                return BadRequest("You are not in this course");
                            }
                        }
                        else
                        {
                            return NotFound("Course not found");
                        }

                    }
                }
                IQueryable<Report> query = await _reportRepo.GetRelation<Report>(null,
                    new List<Expression<Func<Report, bool>>?>
                        {
                        Improved != null ? (Expression<Func<Report, bool>>)(ta => ta.Improved == isImproved) : null,
                        CourseId.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.Student.CourseID == CourseId) : null,
                        Teacher ? (Expression<Func<Report, bool>>)(x => x.UserId == userId) : null
                    }.Where(x => x != null).Cast<Expression<Func<Report, bool>>>().ToList()
                    );
                if (!query.Any())
                {
                    return NotFound("No reports found.");
                }

                var res = await _reportRepo.GetPagedDataAsync(query.OrderByDescending(report => report.CreationDate), paginationParameters);
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "GetStudentsReports")]

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            try
            {
                var report = await _reportRepo.GetRelation<Report>(x => x.Id == id);
                if (!report.Any())
                {
                    return BadRequest($"no report has an ID {id}");
                }
                var userId = _reportRepo.GetUserIDFromToken(User);
                var Teacher = await _reportRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    report = report.Where(x => x.UserId == userId);
                    if (!report.Any())
                    {
                        return BadRequest("This report is not for you");
                    }
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "GetStudentsReports")]

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
            try
            {
                IQueryable<Report> query = (await _reportRepo.GetRelation<Report>(null,
                new List<Expression<Func<Report, bool>>?>
                    {
                        CourseId.HasValue ? (Expression<Func<Report, bool>>)(x => x.Student.CourseID == CourseId) : null,
                        !string.IsNullOrEmpty(teacherId) ? (Expression<Func<Report, bool>>)(x => x.UserId == teacherId) : null,
                        studentId.HasValue ? (Expression<Func<Report, bool>>)(x => x.StudentId == studentId) : null,
                        reportId.HasValue ? (Expression<Func<Report, bool>>)(x => x.Id == reportId) : null,
                        startDate.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.CreationDate >= startDate.Value) : null,
                        endDate.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.CreationDate <= endDate.Value) : null,
                        year.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.CreationDate.Year == year.Value) : null,
                        month.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.CreationDate.Month == month.Value) : null,
                        dateTime.HasValue ? (Expression<Func<Report, bool>>)(sa => sa.CreationDate.Date == dateTime.Value.Date) : null,
                        !string.IsNullOrEmpty(Improved) && Improved.ToLower() == "null" ? (Expression<Func<Report, bool>>)(sa => sa.Improved == null) : null,
                        !string.IsNullOrEmpty(Improved) && Improved.ToLower() == "true" ? (Expression<Func<Report, bool>>)(sa => sa.Improved == true) : null,
                        !string.IsNullOrEmpty(Improved) && Improved.ToLower() == "false" ? (Expression<Func<Report, bool>>)(sa => sa.Improved == false) : null,
                    }.Where(x => x != null).Cast<Expression<Func<Report, bool>>>().ToList())).OrderByDescending(report => report.CreationDate);

                if (!query.Any())
                {
                    return NotFound("No reports found.");
                }
                var userId = _reportRepo.GetUserIDFromToken(User);
                var Teacher = await _reportRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    query = query.Where(x => x.UserId == userId);
                    if (!query.Any())
                    {
                        return BadRequest("No report found");
                    }
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            

        }

        [Authorize(Policy = "ManageChildren")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Report>>> GetReportsByStudent([FromQuery] int studentId)
        {
            try
            {
                var ParentId = _reportRepo.GetUserIDFromToken(User);
                var student = await _studentRepo.GetById(studentId);
                if (student == null)
                {
                    return NotFound("Student not found");
                }
                if (student.ParentID != ParentId)
                {
                    return BadRequest("This student is not the User's child");
                }
                IQueryable<Report> query = (await _reportRepo.GetRelation<Report>(x => x.StudentId == studentId && x.Improved == true))
                    .OrderByDescending(report => report.CreationDate);
                if (query == null)
                {
                    return NotFound("No reports found.");
                }

                var result = query.Select(x => new
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

                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "ManageChildren")]

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetOneReport(int id)
        {
            try
            {
                var report = await _reportRepo.GetRelation<Report>(x => x.Id == id && x.Improved == true);
                if (!report.Any())
                {
                    return BadRequest($"no report has an ID {id}");
                }
                var ParentId = _reportRepo.GetUserIDFromToken(User);
                var student = await _studentRepo.GetById(report.First().StudentId ?? 0);
                if (student == null)
                {
                    return NotFound("Student not found");
                }
                if (student.ParentID != ParentId)
                {
                    return BadRequest("This student is not the User's child");
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

                }).FirstOrDefault());
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "InsertUpdateStudentsReports")]
        [HttpPost]
        public async Task<IActionResult> CreateReports(List<Map_Report_Post> map_Report)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var DateNow = DateTime.Now;
                var studentIds = new HashSet<int>(map_Report.Select(x => x.StudentId));

                var firstDayOfMonth = new DateTime(DateNow.Year, DateNow.Month, 1);
                var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

                var reports = (await _reportRepo.GetRelation<Report>(
                                    report => report.CreationDate >= firstDayOfMonth &&
                                     report.CreationDate < firstDayOfNextMonth &&
                                     studentIds.Contains(report.StudentId ?? 0)))
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

                await _reportRepo.ManyAdd(AllReports);
                await _emailSender.SendToSecretaryNewReportsForStudents();
                return Ok(AllReports.Select(x => new {
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
            
        }
        [Authorize(Policy = "ApproveReports")]

        [HttpPatch("AcceptReport")]
        public async Task<IActionResult> AcceptReport(int ReportId)
        {
            try
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "ApproveReports")]
        [HttpPatch("UnAcceptReport")]
        public async Task<IActionResult> UnAcceptReport(int ReportId)
        {
            try
            {
                var report = await _reportRepo.GetById(ReportId);
                if (report == null)
                {
                    return BadRequest("This report not found");
                }
                report.Improved = false;
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ApproveReports")]

        [HttpPatch("AcceptAllReport")]
        public async Task<IActionResult> AcceptAllReport(List<int> ReportIds)
        {
            try
            {
                var reports = await _reportRepo.GetRelation<Report>(x => ReportIds.Contains(x.Id) && x.Improved == null);
                if (!reports.Any())
                {
                    return BadRequest("This reports not found");
                }
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "InsertUpdateStudentsReports")]
        [HttpPatch("[action]")]
        public async Task<IActionResult> UpdateReport(Map_Report_update map_Report_Update)
        {
            try
            {
                var report = await _reportRepo.GetById(map_Report_Update.Id);
                if (report == null)
                {
                    return BadRequest("This report not found");
                }
                if (report.Improved == true)
                {
                    return BadRequest("This report has already been approved");
                }
                report.Improved = null;
                report.data = map_Report_Update.Data;
                await _reportRepo.Update(report);
                await _emailSender.SendToSecretaryUpdateReportsForStudents();
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "ExportReport")]

        [HttpPost("ExportReport")]
        public async Task<IActionResult> ExportReport(int reportId)
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
                var reportBase64 = await _exportPDF.ExportReport(reportId);

                return Ok(reportBase64);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            

        }
        //[AllowAnonymous]
        //[HttpPost("test")]
        //public async Task<IActionResult> DownloadReport(int id)
        //{
        //    var x = await _exportPDF.ExportReport(id);
        //    byte[] bytes = Convert.FromBase64String(x);
        //    return File(x, "application/pdf", "report.pdf");
        //}
    }
}
