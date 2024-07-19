using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;


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

        private readonly ILogger<ReportsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public ReportsController(IRepository<Report> reportRepo, IRepository<SchoolInfo> schoolInfo,
            IRepository<Student> studentRepo, IEmailSender emailSender,IMapper mapper, ILogger<ReportsController> logger)
        {
            _reportRepo = reportRepo;
            _schoolInfo = schoolInfo;
            _studentRepo = studentRepo;
            _emailSender = emailSender;
            _mapper = mapper;
            _logger = logger;
        }
        
        [HttpGet("Keys")]
        public async Task<IActionResult> GetReportKeys()
        {
            try
            {
                var keys = (await _schoolInfo.GetRelation()).Select(x => x.StudentsReportsKeys).SingleOrDefault();
                return Ok(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Report>>> GetAllReports([FromQuery] bool? Improved)
        {
            try
            {
                IQueryable<Report> query = (await _reportRepo.GetRelation()).Where(sa => sa.Improved == Improved);
                var result = query.Select(x => new
                {
                    x.Id,
                    x.data,
                    x.CreationDate,
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
                }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("Filter")]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports(
            [FromQuery] string? teacherId,
            [FromQuery] int? studentId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] DateTime? dateTime,
            [FromQuery] bool? Improved
            )
        {
            try
            {
                IQueryable<Report> query = (await _reportRepo.GetRelation()).Where(sa => sa.Improved == Improved);
                if (query == null)
                {
                    return NotFound("No reports found.");
                }
                if (!string.IsNullOrEmpty(teacherId))
                {
                    query = query.Where(x => x.UserId == teacherId);
                }
                if (studentId.HasValue)
                {
                    query = query.Where(x => x.StudentId == studentId);
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

                if (!query.Any())
                {
                    return NotFound("No reports found.");
                }
                var result = query.Select(x => new
                {
                    x.Id,
                    x.data,
                    x.CreationDate,
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
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "ReportsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReports(List<Map_Report> map_Report)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var mapDates = new HashSet<(int Year, int Month, int StudentId)>(
                    map_Report.Select(x => (x.CreationDate.Year, x.CreationDate.Month, x.StudentId))
                );

                var reports = (await _reportRepo.GetAll())
                    .Where(d => mapDates.Contains((d.CreationDate.Year, d.CreationDate.Month, d.StudentId ?? 0)))
                    .ToList();

                if (reports.Any())
                {
                    return BadRequest($"There is a report for the student with Id: {reports[0].StudentId} on this date {reports[0].CreationDate.Month}/{reports[0].CreationDate.Year}");
                }
                //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //if (string.IsNullOrEmpty(userId))
                //{
                //    return Unauthorized("User ID not found in token.");
                //}
                //UserId = userId;
                List<Report> AllReports = map_Report.Select(map => new Report
                {
                    CreationDate = map.CreationDate,
                    StudentId = map.StudentId,
                    data = map.data,
                    Improved = null,
                    UserId = map.UserId//
                }).ToList();

                await _reportRepo.ManyAdd(AllReports);
                await _emailSender.SendToSecretaryNewReportsForStudents();
                return Ok(AllReports.Select(x => new { x.Id, x.CreationDate, x.data, x.Improved, x.UserId, x.StudentId }).ToList());

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "ReportsController", ex.Message));
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }
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
            report.Improved = true;

                await _reportRepo.Update(report);
                var student = await _studentRepo.GetById(report.StudentId ?? 0);
                await _emailSender.SendToParentsNewReportsForStudents([student]);
                return Ok(new { report.Id, report.CreationDate, report.data, report.StudentId, report.UserId,report.Improved});
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "ReportsController", ex.Message));
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
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
                return Ok(new { report.Id, report.CreationDate, report.data, report.StudentId, report.UserId, report.Improved });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "ReportsController", ex.Message));
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        [HttpPatch("AcceptAllReport")]
        public async Task<IActionResult> AcceptAllReport(List<int> ReportIds)
        {
            try
            {
                var AllReports = await _reportRepo.GetRelation();
                var reports = AllReports.Where(x => ReportIds.Contains(x.Id));
                if (!reports.Any())
                {
                    return BadRequest("This reports not found");
                }

                await reports.ForEachAsync(report => report.Improved = true);
                await _reportRepo.ManyUpdate(reports);

                List<Student> students = reports.Select(x => x.Student).ToList();
                await _emailSender.SendToParentsNewReportsForStudents(students);
                return Ok(reports.Select(x => new { x.Id, x.CreationDate, x.data, x.StudentId, x.UserId, x.Improved }));
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "ReportsController", ex.Message));
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
