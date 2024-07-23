using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        public ReportsController(IRepository<Report> reportRepo, IRepository<SchoolInfo> schoolInfo,
            IRepository<Student> studentRepo, IEmailSender emailSender,IMapper mapper)
        {
            _reportRepo = reportRepo;
            _schoolInfo = schoolInfo;
            _studentRepo = studentRepo;
            _emailSender = emailSender;
            _mapper = mapper;
        }
        
        [HttpGet("Keys")]
        public async Task<IActionResult> GetReportKeys()
        {
            var keys = (await _schoolInfo.GetRelation()).Select(x => x.StudentsReportsKeys).SingleOrDefault();
            return Ok(keys);
        }
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Report>>> GetAllReports([FromQuery] string? Improved, int? CourseId)
        {
            IQueryable<Report> query;
            
            if (Improved == null) 
            {
                query = await _reportRepo.GetRelation();
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
                query = (await _reportRepo.GetRelation()).Where(sa => sa.Improved == isImproved);
            }
            if (CourseId.HasValue)
            {
                query = query.Where(x => x.Student.CourseID == CourseId);
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
                x.Improved
            }).ToList();
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
            [FromQuery] string? Improved
            )
        {
            IQueryable<Report> query = await _reportRepo.GetRelation();
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
                x.Improved
            }).ToList();

            return Ok(result);

        }

        [HttpPost]
        public async Task<IActionResult> CreateReports(List<Map_Report_Post> map_Report)
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
            var userId = _reportRepo.GetUserIDFromToken(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            List<Report> AllReports = map_Report.Select(map => new Report
            {
                CreationDate = map.CreationDate,
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
        public async Task<IActionResult> UpdateReport(int ReportId)
        {
            var report = await _reportRepo.GetById(ReportId);
            if (report == null)
            {
                return BadRequest("This report not found");
            }
            if (report.Improved != false)
            {
                if(report.Improved == null)
                {
                    return BadRequest("This report needs to not approved first");
                }
                else
                {
                    return BadRequest("This report has already been approved");
                }
            }
            report.Improved = null;
            try
            {
                await _reportRepo.Update(report);
                var student = await _studentRepo.GetById(report.StudentId ?? 0);
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
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
