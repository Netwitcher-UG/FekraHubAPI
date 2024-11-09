using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Controllers.AuthorizationController;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace FekraHubAPI.Controllers.Attendance
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class AttendanceController : ControllerBase
    {
        private readonly IRepository<TeacherAttendance> _teacherAttendanceRepo;
        private readonly IRepository<StudentAttendance> _studentAttendanceRepo;
        private readonly IRepository<Course> _coursRepo;
        private readonly IRepository<Event> _eventRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<AttendanceStatus> _attendanceStatusRepo;
        private readonly IRepository<AttendanceDate> _attendanceDateRepo;
        private readonly IRepository<CourseAttendance> _courseAttendanceRepo;
        private readonly IRepository<CourseSchedule> _courseScheduleRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthorizationUsersController> _logger;
        private readonly IContractMaker _contractMaker;

        public AttendanceController(IRepository<TeacherAttendance> teacherAttendanceRepo,
            IRepository<StudentAttendance> studentAttendanceRepo,
            IRepository<Course> coursRepo,
            IRepository<AttendanceStatus> attendanceStatusRepo,
            IRepository<Student> studentRepo, IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IRepository<AttendanceDate> attendanceDateRepo,
            IRepository<CourseAttendance> courseAttendanceRepo,
            IRepository<CourseSchedule> courseScheduleRepo,
            ILogger<AuthorizationUsersController> logger, IRepository<Event> eventRepo,
            IContractMaker contractMaker)
        {
            _teacherAttendanceRepo = teacherAttendanceRepo;
            _studentAttendanceRepo = studentAttendanceRepo;
            _coursRepo = coursRepo;
            _studentRepo = studentRepo;
            _attendanceStatusRepo = attendanceStatusRepo;
            _mapper = mapper;
            _userManager = userManager;
            _attendanceDateRepo = attendanceDateRepo;
            _courseAttendanceRepo = courseAttendanceRepo;
            _courseScheduleRepo = courseScheduleRepo;
            _logger = logger;
            _eventRepo = eventRepo;
            _contractMaker = contractMaker;
        }



        [Authorize]
        [HttpGet("ExportAttendanceReport")]
        public async Task<IActionResult> ExportMonthlyAttendanceReportPDF([Required]int courseId , DateTime? date)//////////////////////////////////////////
        {
            var course = await _coursRepo.GetRelationSingle(
                where:x=>x.Id == courseId,
                include:x=>x.Include(z=>z.Student).ThenInclude(z=>z.StudentAttendance).ThenInclude(z=>z.AttendanceStatus)
                .Include(z=>z.Room).Include(z=>z.CourseSchedule).Include(z=>z.Teacher),
                selector:x=> x,
                
                returnType:QueryReturnType.FirstOrDefault,
                asNoTracking:true
                );
            if (course == null)
            {
                return BadRequest("Die Kurse wurden nicht gefunden.");//course not found
            }
            if(course.Student.Count == 0)
            {
                return BadRequest("In diesem Kurs sind keine Schüler registriert");//no student in course
            }
            if (course.CourseSchedule.Count == 0)
            {
                return BadRequest("Es ist keine Anwesenheit für diesen Kurs registriert.");
            }
            string? pdf;
            if(!date.HasValue)
            {
                pdf = await _contractMaker.AttendanceReport(course);
            }
            else
            {
                pdf = await _contractMaker.MonthlyAttendanceReport(course, date.Value);
            }
            
           

            return Ok(pdf);
        }
        [Authorize]
        [HttpGet("CourseWorkingDates")]
        public async Task<IActionResult> CourseWorkingDates([Required] int courseId)
        {
            
            try
            {
                var course = await _coursRepo.GetRelationSingle(
                    where: x => x.Id == courseId,
                    include: x => x.Include(z => z.CourseSchedule),
                    selector: x => new
                    {
                        x.StartDate,
                        x.EndDate,
                        StudentsCount = x.Student.Count(), 
                        CourseSchedule = x.CourseSchedule.Select(cs => cs.DayOfWeek)
                    },
                    returnType: QueryReturnType.FirstOrDefault,
                    asNoTracking: true
                );

                if (course == null)
                {
                    return BadRequest("Die Kurse wurden nicht gefunden."); 
                }

                if (course.StudentsCount == 0)
                {
                    return BadRequest(new List<object>()); 
                }

                DateTime startDate = course.StartDate.Date;
                DateTime endDate = course.EndDate.Date < DateTime.Now.Date ? course.EndDate.Date : DateTime.Now.Date;

                var workingDays = course.CourseSchedule.ToList();

                if (workingDays.Count == 0)
                {
                    return BadRequest(new List<object>()); 
                }

                var workingDates = new List<DateTime>();
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (workingDays.Contains(date.DayOfWeek.ToString()))
                    {
                        workingDates.Add(date);
                    }
                }

                var groupedDates = workingDates
                    .GroupBy(d => new { d.Year, d.Month })
                    .Select(g => new
                    {
                        key = $"{g.Key.Month:D2}.{g.Key.Year}",
                        value = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM-dd")
                    })
                    .ToList();

                return Ok(groupedDates);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

    




        [Authorize]
        [HttpGet("AttendanceStatus")]
        public async Task<ActionResult<IEnumerable<AttendanceStatus>>> GetAttendanceStatus()
        {
            try
            {
                IEnumerable<AttendanceStatus> attendanceStatus = await _attendanceStatusRepo.GetAll();
                //if (!attendanceStatus.Any())
                //{
                //    return Ok(new List<AttendanceStatus>());
                //}
                return Ok(attendanceStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageAttendanceStatus")]
        [HttpPost("AttendanceStatus")]
        public async Task<IActionResult> AddAttendanceStatus([FromForm] string status)
        {
            try
            {
                if (status == null)
                {
                    return BadRequest("Bitte geben Sie einen Status ein.");//Please enter a status
                }
                var statuses = await _attendanceStatusRepo.GetRelationSingle(
                    where: s => s.Title.ToLower() == status.ToLower(),
                    selector:x=>x,
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true);
                if (statuses != null)
                {
                    return BadRequest("Dieser Status existiert bereits.");//This status is already exists
                }
                AttendanceStatus attendanceStatus = new AttendanceStatus()
                {
                    Title = status,
                };

                await _attendanceStatusRepo.Add(attendanceStatus);
                return Ok(attendanceStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
            

        }

        [Authorize(Policy = "ManageAttendanceStatus")]
        [HttpDelete("AttendanceStatus/{id}")]
        public async Task<IActionResult> DeleteAttendanceStatus(int id)
        {
            try
            {
                var attendanceStatus = await _attendanceStatusRepo.GetById(id);
                if (attendanceStatus == null)
                {
                    return BadRequest("Der Status mit der angegebenen ID existiert nicht.");//The status with the provided ID does not exist.
                }
                await _attendanceStatusRepo.Delete(attendanceStatus.Id);

                return Ok($"{attendanceStatus.Title} erfolgreich gelöscht.");//deleted successfully.
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
    }
}
