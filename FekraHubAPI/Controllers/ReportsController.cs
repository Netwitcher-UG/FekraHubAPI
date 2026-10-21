using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.ExportReports;
using FekraHubAPI.Helpers;
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
                var keys = await _studentsReportsKeyInfo.GetRelationList(
                    selector: x => x.Keys,
                    asNoTracking:true);
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
                        var teacherIds = await _courseRepo.GetRelationSingle(
                            where: x => x.Id == CourseId,
                            returnType:QueryReturnType.SingleOrDefault,
                            include:x=>x.Include(t=>t.Teacher),
                            selector:x=>x.Teacher.Select(x => x.Id).ToList());

                        if (teacherIds != null && !teacherIds.Contains(userId))
                        {
                            return BadRequest("Sie sind nicht in diesem Kurs.");//You are not in this course
                        }



                    }
                }
                var query = await _reportRepo.GetRelationAsQueryable(
                    manyWhere: new List<Expression<Func<Report, bool>>?>
                        {
                        Improved != null ? (Expression<Func<Report, bool>>)(ta => ta.Improved == isImproved) : null,
                        CourseId.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.Student.CourseID == CourseId) : null,
                        Teacher ? (Expression<Func<Report, bool>>)(x => x.UserId == userId) : null
                    }.Where(x => x != null).Cast<Expression<Func<Report, bool>>>().ToList(),
                    orderBy: x => x.CreationDate,
                    include:x=>x.Include(z=>z.Student).ThenInclude(z=>z.Course),
                    selector: x => new
                    {
                        x.Id,
                        x.data,
                        x.CreationDate,
                        Year = x.ReportYear,
                        Month = x.ReportMonth,
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
                    },
                    asNoTracking:true
                    );
                if (!query.Any())
                {
                    return BadRequest("Keine Berichte gefunden.");//No reports found.
                }

                var res = await _reportRepo.GetPagedDataAsync(query, paginationParameters);
                return Ok(new
                {
                    res.CurrentPage,
                    res.PageSize,
                    res.TotalCount,
                    res.TotalPages,
                    res.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "GetTeacher")]
        [HttpGet("TeacherProfile")]
        public async Task<IActionResult> GetTeacherReport(string id)
        {
            var Teacher = await _Users.FindByIdAsync(id); 
            if (Teacher == null)
            {
                return BadRequest("Lehrer nicht gefunden.");//Teacher not found
            }
            var isTeacher = await _reportRepo.IsTeacherIDExists(id);
            if (!isTeacher)
            {
                return BadRequest("Die Report-ID gehört nicht zu einem Lehrer.");//The ReportId does not belong to a teacher
            }
            var teacherReports = await _reportRepo.GetRelationList(
                where: x=> x.UserId == id ,
                asNoTracking:true,
                include:x=> x.Include(z=>z.Student).ThenInclude(z=>z.Course),
                selector:x=>new
                {
                    x.Id,
                    x.CreationDate,
                    Year = x.ReportYear,
                    Month = x.ReportMonth,
                    x.data,
                    x.Improved,
                    Student = new
                    {
                        x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName,
                        Course = new
                        {
                            x.Student.Course.Id,
                            x.Student.Course.Name
                        }
                    }
                }
                );
            return Ok(new { Teacher = new { Teacher.Id, Teacher.FirstName, Teacher.LastName },teacherReports });
        }

        [Authorize(Policy = "GetStudentsReports")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            try
            {
                var report = await _reportRepo.GetRelationSingle(
                    where: x => x.Id == id,
                    include:x=>x.Include(u=>u.User).Include(s=>s.Student).ThenInclude(c=>c.Course),
                    selector: x => new
                    {
                        x.Id,
                        x.data,
                        x.CreationDate,
                        Year = x.ReportYear,
                        Month = x.ReportMonth,
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
                    },
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true);

                if (report == null)
                {
                    return BadRequest($"Kein Bericht hat die ID {id}.");//no report has an ID {id}
                }
                var userId = _reportRepo.GetUserIDFromToken(User);
                var Teacher = await _reportRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    if (report.TeacherId != userId)
                    {
                        return BadRequest("Dieser Bericht ist nicht für Sie.");//This report is not for you
                    }
                }
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "GetStudentsReports")]

        [HttpGet("Filter")]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports(//fixe for teacher
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
                var userId = _reportRepo.GetUserIDFromToken(User);
                var Teacher = await _reportRepo.IsTeacherIDExists(userId);
                DateTime? startUtc = startDate?.ToUtcSafe();
                DateTime? endUtc = endDate?.ToUtcSafe();
                DateTime? dateOnlyUtc = dateTime.HasValue ? dateTime.Value.Date.ToUtcSafe() : null;
                var query = await _reportRepo.GetRelationAsQueryable(
                manyWhere: new List<Expression<Func<Report, bool>>?>
            {
                Teacher ? (Expression<Func<Report, bool>>)(x => x.UserId == userId) : null,
                CourseId.HasValue ? (Expression<Func<Report, bool>>)(x => x.Student.CourseID == CourseId) : null,
                !string.IsNullOrEmpty(teacherId) ? (Expression<Func<Report, bool>>)(x => x.UserId == teacherId) : null,
                studentId.HasValue ? (Expression<Func<Report, bool>>)(x => x.StudentId == studentId) : null,
                reportId.HasValue ? (Expression<Func<Report, bool>>)(x => x.Id == reportId) : null,
                startUtc.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.CreationDate >= startUtc.Value) : null,
                endUtc.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.CreationDate <= endUtc.Value) : null,
                year.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.ReportYear == year.Value) : null,
                month.HasValue ? (Expression<Func<Report, bool>>)(ta => ta.ReportMonth == month.Value) : null,
                dateOnlyUtc.HasValue ? (Expression<Func<Report, bool>>)(sa => sa.CreationDate.Date == dateOnlyUtc.Value) : null,
                !string.IsNullOrEmpty(Improved) && Improved.ToLower() == "null" ? (Expression<Func<Report, bool>>)(sa => sa.Improved == null) : null,
                !string.IsNullOrEmpty(Improved) && Improved.ToLower() == "true" ? (Expression<Func<Report, bool>>)(sa => sa.Improved == true) : null,
                !string.IsNullOrEmpty(Improved) && Improved.ToLower() == "false" ? (Expression<Func<Report, bool>>)(sa => sa.Improved == false) : null,
            }.Where(x => x != null).Cast<Expression<Func<Report, bool>>>().ToList(),
                orderBy: x => x.CreationDate,
                include: x => x.Include(u => u.User).Include(s => s.Student).ThenInclude(c => c.Course),
                selector: x => new
                {
                    x.Id,
                    x.data,
                    x.CreationDate,
                    Year = x.ReportYear,
                    Month = x.ReportMonth,
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
                },
                asNoTracking:true);

                //if (!query.Any())
                //{
                //    return BadRequest("Keine Berichte gefunden.");//No reports found.
                //}
                var res = await _reportRepo.GetPagedDataAsync(query, paginationParameters);

                return Ok(new
                {
                    res.CurrentPage,
                    res.PageSize,
                    res.TotalCount,
                    res.TotalPages,
                    res.Data
                });
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
                var student = await _studentRepo.GetRelationSingle(
                        where: x =>
                            x.Id == studentId &&
                            x.ParentID == ParentId &&
                            x.ActiveStudent == true &&
                            x.AdminApproved == true &&
                            x.ParentApproved == true,

                        selector: x => x,
                        returnType: QueryReturnType.SingleOrDefault,
                        asNoTracking: true
                    );
                if (student == null)
                {
                    return BadRequest("Schüler nicht gefunden");//Student not found
                }
                if (student.ParentID != ParentId)
                {
                    return BadRequest("Dieser Schüler ist nicht das Kind des Benutzers.");//This student is not the User's child
                }
                var result = await _reportRepo.GetRelationList(
                    where: x => x.StudentId == studentId && x.Improved == true,
                    orderBy: x => x.CreationDate,
                    include: x => x.Include(u => u.User).Include(s => s.Student).ThenInclude(c => c.Course),
                    selector: x => new
                    {
                        x.Id,
                        x.data,
                        x.CreationDate,
                        Year = x.ReportYear,
                        Month = x.ReportMonth,
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

                    },
                    asNoTracking:true);
                if (!result.Any())
                {
                    return BadRequest("Keine Berichte gefunden.");//No reports found.
                }

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
                var report = await _reportRepo.GetRelationSingle(
                    where:x => x.Id == id && x.Improved == true,
                    include: x => x.Include(u => u.User).Include(s => s.Student).ThenInclude(c => c.Course),
                    selector: x => new
                    {
                        x.Id,
                        x.data,
                        x.CreationDate,
                        Year = x.ReportYear,
                        Month = x.ReportMonth,
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

                    },
                    asNoTracking:true);
                if (report == null)
                {
                    return BadRequest($"Kein Bericht hat die ID {id}.");//no report has an ID {id}
                }
                var ParentId = _reportRepo.GetUserIDFromToken(User);
                var student = await _studentRepo.GetById(report.Student.Id);
                if (student.ParentID != ParentId ||
                    student.ActiveStudent == false ||
                    student.AdminApproved == false ||
                    student.ParentApproved == false)
                {
                    return BadRequest("Schüler nicht gefunden");
                }
                if (student == null)
                {
                    return BadRequest("Schüler nicht gefunden");//Student not found
                }
                if (student.ParentID != ParentId)
                {
                    return BadRequest("Dieser Schüler ist nicht das Kind des Benutzers.");//This student is not the User's child
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        //[Authorize(Policy = "InsertUpdateStudentsReports")]
        [HttpPost]
        public async Task<IActionResult> CreateReports([FromQuery] int month, [FromQuery] int year, List<Map_Report_Post> map_Report)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (month < 1 || month > 12 || year < 1 || year > 9999)
                {
                    return BadRequest("Ungültiger Monat oder ungültiges Jahr.");
                }

                if (map_Report == null || !map_Report.Any())
                {
                    return BadRequest("Es wurden keine Berichte übermittelt.");
                }

                if (map_Report.Any(x => x.StudentId <= 0))
                {
                    return BadRequest("Ungültige Schüler-ID.");
                }

                var duplicateStudentIds = map_Report
                    .GroupBy(x => x.StudentId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateStudentIds.Any())
                {
                    return BadRequest(new
                    {
                        Message = "Ein Schüler wurde mehrfach ausgewählt.",
                        StudentIds = duplicateStudentIds
                    });
                }
                var DateNow = DateTime.UtcNow;
                var studentIds = new HashSet<int>(map_Report.Select(x => x.StudentId));

                var reports = await _reportRepo.DataExist(
                    report =>
                        report.ReportMonth == month &&
                        report.ReportYear == year &&
                        studentIds.Contains(report.StudentId ?? 0)
                );

                if (reports)
                {
                    return Conflict(new
                    {
                        Message = $"Für {month:D2}/{year} existiert bereits ein Bericht für mindestens einen Schüler.",
                        Month = month,
                        Year = year
                    });
                }
                var userId = _reportRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Benutzer-ID nicht im Token gefunden.");//User ID not found in token.
                }
                List<Report> AllReports = map_Report.Select(map => new Report
                {
                    CreationDate = DateNow,
                    ReportMonth = month,
                    ReportYear = year,
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
                    Year = x.ReportYear,
                    Month = x.ReportMonth,
                    x.Improved,
                    x.UserId,
                    x.StudentId
                }).ToList());
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is Npgsql.PostgresException pg &&
                pg.SqlState == Npgsql.PostgresErrorCodes.UniqueViolation &&
                pg.ConstraintName == "UX_Reports_Student_Period")
            {
                return Conflict(new
                {
                    Message = "Für diesen Schüler existiert bereits ein Bericht für diesen Monat und dieses Jahr."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    HandleLogFile.handleErrLogFile(
                        User, "ReportsController", ex.Message));

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
                    return BadRequest("Dieser Bericht wurde nicht gefunden.");//This report not found
                }
                if (report.Improved == false)
                {
                    return BadRequest("Dieser Bericht muss zuerst aktualisiert werden.");//This report needs to updates first
                }
                report.Improved = true;
                await _reportRepo.Update(report);
                var student = await _studentRepo.GetById(report.StudentId ?? 0);
                await _emailSender.SendToParentsNewReportsForStudents([student]);
                return Ok(new { 
                    report.Id,
                    report.CreationDate,
                    Year = report.ReportYear,
                    Month = report.ReportMonth,
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
                    return BadRequest("Dieser Bericht wurde nicht gefunden.");//This report not found
                }
                report.Improved = false;
                await _reportRepo.Update(report);
                await _emailSender.SendToTeacherReportsForStudentsNotAccepted(report.StudentId ?? 0, report.UserId ?? "");
                return Ok(new {
                    report.Id,
                    report.CreationDate,
                    Year = report.ReportYear,
                    Month = report.ReportMonth,
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
                if(ReportIds == null || !ReportIds.Any())
                {
                    return BadRequest("Keine Berichte ausgewählt.");//No reports selected
                }
                var reports = await _reportRepo.GetRelationAsQueryable(
                    where:x => ReportIds.Contains(x.Id) && x.Improved == null,
                    include:x=>x.Include(z=>z.Student),
                    selector:x=>x);
                if (!reports.Any())
                {
                    return BadRequest("Dieser Bericht wurde nicht gefunden.");//This reports not found
                }
                List<Student> students = reports.Select(x => x.Student).ToList();
                foreach (var report in reports)
                {
                    report.Improved = true;
                }
                await _reportRepo.ManyUpdate(reports);

                
                await _emailSender.SendToParentsNewReportsForStudents(students);
                return Ok(reports.Select(x => new {
                    x.Id,
                    x.CreationDate,
                    Year = x.ReportYear,
                    Month = x.ReportMonth,
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
                    return BadRequest("Dieser Bericht wurde nicht gefunden.");//This report not found
                }
                if (report.Improved == true)
                {
                    return BadRequest("Dieser Bericht wurde bereits genehmigt");//This report has already been approved
                }
                report.Improved = null;
                report.data = map_Report_Update.Data;
                await _reportRepo.Update(report);
                await _emailSender.SendToSecretaryUpdateReportsForStudents();
                return Ok(new {
                    report.Id,
                    report.CreationDate,
                    Year = report.ReportYear,
                    Month = report.ReportMonth,
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
                    return BadRequest("Keine Berichte gefunden.");//no report found
                }
                if (report.Improved != true)
                {
                    return BadRequest("Dieser Bericht wurde nicht genehmigt.");//this report was not approved
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
