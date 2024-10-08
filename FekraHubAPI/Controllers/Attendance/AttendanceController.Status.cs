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



        [AllowAnonymous]
        [HttpGet("test")]
        public async Task<IActionResult> testPDF(int courseId , DateTime date)
        {
            var course = await _coursRepo.GetRelationSingle(
                where:x=>x.Id == courseId,
                include:x=>x.Include(z=>z.Student).ThenInclude(z=>z.StudentAttendance).ThenInclude(z=>z.AttendanceStatus)
                .Include(z=>z.Room).Include(z=>z.CourseSchedule).Include(z=>z.Teacher),
                selector:x=> x,
                
                returnType:QueryReturnType.FirstOrDefault
                );
            if (course == null)
            {
                return BadRequest("course not found");//////////////////
            }
            var student = await _studentRepo.GetById(61);
            var pdf = await _contractMaker.AttendanceReport(course,date);
            byte[] pdfBytes = Convert.FromBase64String(pdf);

            return File(pdfBytes, "application/pdf", "contract.pdf");
        }




        [Authorize(Policy = "ManageAttendanceStatus")]
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
