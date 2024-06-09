using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;


namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IRepository<Report> _reportRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<SchoolInfo> _schoolInfo;
        private readonly IEmailSender _emailSender;
        public ReportsController(IRepository<Report> reportRepo, IRepository<SchoolInfo> schoolInfo,
            IRepository<Student> studentRepo, IEmailSender emailSender)
        {
            _reportRepo = reportRepo;
            _schoolInfo = schoolInfo;
            _studentRepo = studentRepo;
            _emailSender = emailSender;
        }
        
        [HttpGet("Keys")]
        public async Task<IActionResult> GetReportKeys()
        {
            var keys = (await _schoolInfo.GetRelation()).Select(x => x.StudentsReportsKeys).SingleOrDefault();
            return Ok(keys);
        }
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Report>>> GetAllReports([FromQuery] bool? Improved)
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
                x.StudentId,
                StudentFirstName = x.Student.FirstName,
                StudentLastName = x.Student.LastName,
                x.Improved
            }).ToList();
            return Ok(result);
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
                x.StudentId,
                StudentFirstName = x.Student.FirstName,
                StudentLastName = x.Student.LastName,
                x.Improved
            }).ToList();

            return Ok(result);

        }

        [HttpPost]
        public async Task<IActionResult> CreateReports(List<Map_Report> map_Report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var mapDates = map_Report.Select(x => new { x.CreationDate.Year, x.CreationDate.Month, x.StudentId }).ToList();
            var reports = (await _reportRepo.GetAll())
                .Where(d => mapDates.Any(md => md.Year == d.CreationDate.Year &&
                md.Month == d.CreationDate.Month && md.StudentId == d.StudentId)).ToList();
            if (reports.Any())
            {
                return BadRequest($"There is a report for the student with Id: {reports[0].Id}  on this date {reports[0].CreationDate.Month}/{reports[0].CreationDate.Year}");
            }
            foreach (var map in map_Report)
            {
                var report = new Report()
                {
                    CreationDate = map.CreationDate,
                    StudentId = map.StudentId,  
                    data = map.data,
                    Improved = null,
                    UserId = map.UserId
                };

                await _reportRepo.ManyAdd(report);
            }

            await _reportRepo.SaveManyAdd();
            await _emailSender.SendToSecretaryNewReportsForStudents();
            return Ok(map_Report);
        }
        [HttpPatch("AcceptReport")]
        public async Task<IActionResult> AcceptReport(int ReportId)
        {
            var report = await _reportRepo.GetById(ReportId);
            if (report == null)
            {
                return BadRequest("This report not found");
            }
            report.Improved = true;
            try
            {
                await _reportRepo.Update(report);
                var student = await _studentRepo.GetById(report.StudentId ?? 0);
                await _emailSender.SendToParentsNewReportsForStudents([student.ParentID ?? ""]);
                return Ok($"Report accepted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                await _emailSender.SendToTeacherReportsForStudentsNotAccepted(report.StudentId ?? 0);
                return Ok($"Report not accepted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpPatch("AcceptAllReport")]
        public async Task<IActionResult> AcceptAllReport(List<int> ReportIds)
        {
            var AllReports = await _reportRepo.GetRelation();
            var reports = AllReports.Where(x => ReportIds.Contains(x.Id));
            if (!reports.Any())
            {
                return BadRequest("This reports not found");
            }

            
            try
            {
                foreach (var report in reports)
                {
                    report.Improved = true;
                    _reportRepo.ManyUpdate(report);
                }

                await _reportRepo.SaveManyAdd();
                List<string> parentsIds = reports.Select(x => x.UserId).ToList();
                await _emailSender.SendToParentsNewReportsForStudents(parentsIds);
                return Ok($"Reports accepted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
