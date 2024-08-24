using AutoMapper;
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
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<AttendanceStatus> _attendanceStatusRepo;
        private readonly IRepository<AttendanceDate> _attendanceDateRepo;
        private readonly IRepository<CourseAttendance> _courseAttendanceRepo;
        private readonly IRepository<CourseSchedule> _courseScheduleRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(IRepository<TeacherAttendance> teacherAttendanceRepo,
            IRepository<StudentAttendance> studentAttendanceRepo,
            IRepository<Course> coursRepo,
            IRepository<AttendanceStatus> attendanceStatusRepo,
            IRepository<Student> studentRepo, IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IRepository<AttendanceDate> attendanceDateRepo,
            IRepository<CourseAttendance> courseAttendanceRepo,
            IRepository<CourseSchedule> courseScheduleRepo)
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
        }

        [Authorize(Policy = "ManageAttendanceStatus")]
        [HttpGet("AttendanceStatus")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetAttendanceStatus()
        {
            try
            {
                var attendanceStatus = await _attendanceStatusRepo.GetAll();
                if (!attendanceStatus.Any())
                {
                    return Ok("There are no attendance status");
                }
                return Ok(attendanceStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Policy = "ManageAttendanceStatus")]
        [HttpPost("AttendanceStatus")]
        public async Task<IActionResult> AddAttendanceStatus([FromForm] string status)
        {
            if (status == null)
            {
                return BadRequest("Please enter a status");
            }
            var statuses = (await _attendanceStatusRepo.GetRelation<AttendanceStatus>(s => s.Title.ToLower() == status.ToLower())).SingleOrDefaultAsync();
            if (statuses != null)
            {
                return BadRequest("This status is already exists");
            }
            AttendanceStatus attendanceStatus = new AttendanceStatus()
            {
                Title = status,
            };
            try
            {
                await _attendanceStatusRepo.Add(attendanceStatus);
                return Ok(attendanceStatus);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Policy = "ManageAttendanceStatus")]
        [HttpDelete("AttendanceStatus/{id}")]
        public async Task<IActionResult> DeleteAttendanceStatus(int id)
        {

            var attendanceStatus = await _attendanceStatusRepo.GetById(id);
            if (attendanceStatus == null)
            {
                return NotFound("The status with the provided ID does not exist.");
            }


            try
            {
                await _attendanceStatusRepo.Delete(attendanceStatus.Id);

                return Ok($"{attendanceStatus.Title} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
