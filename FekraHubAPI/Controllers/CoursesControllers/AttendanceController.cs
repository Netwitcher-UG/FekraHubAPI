using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IRepository<TeacherAttendance> _teacherAttendanceRepo;
        private readonly IRepository<StudentAttendance> _studentAttendanceRepo;
        private readonly IRepository<Course> _coursRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<AttendanceStatus> _attendanceStatusRepo;
        private readonly IMapper _mapper;

        public AttendanceController(IRepository<TeacherAttendance> teacherAttendanceRepo, IRepository<StudentAttendance> studentAttendanceRepo, IRepository<Course> coursRepo, IRepository<AttendanceStatus> attendanceStatusRepo, IRepository<Student> studentRepo, IMapper mapper)
        {
            _teacherAttendanceRepo = teacherAttendanceRepo;
            _studentAttendanceRepo = studentAttendanceRepo;
            _coursRepo = coursRepo;
            _studentRepo = studentRepo;
            _attendanceStatusRepo = attendanceStatusRepo;
            _mapper = mapper;
        }


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

        [HttpPost("AttendanceStatus")]
        public async Task<IActionResult> AddAttendanceStatus([FromForm] string status)
        {
            if (status == null)
            {
                return BadRequest("Please enter a status");
            }
            var statuses = await _attendanceStatusRepo.GetRelation();
            var Status = await statuses.Where(s => s.Title.ToLower() == status.ToLower()).SingleOrDefaultAsync();
            if (Status != null)
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
                return Ok($"{status} Added seccessfuly");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("AttendanceStatus")]
        public async Task<IActionResult> DeleteAttendanceStatus([FromForm] string? status, int? id)
        {
            if (string.IsNullOrEmpty(status) && id == null)
            {
                return BadRequest("Please provide a status or an ID.");
            }

            var statuses = await _attendanceStatusRepo.GetRelation();

            AttendanceStatus attendanceStatus = null;

            if (id.HasValue)
            {
                attendanceStatus = await _attendanceStatusRepo.GetById(id.Value);
                if (attendanceStatus == null)
                {
                    return NotFound("The status with the provided ID does not exist.");
                }

            }
            else if (!string.IsNullOrEmpty(status))
            {
                attendanceStatus = await statuses.Where(s => s.Title.ToLower() == status.ToLower()).SingleOrDefaultAsync();
                if (attendanceStatus == null)
                {
                    return NotFound("The status with the provided title does not exist.");
                }
            }

            if (attendanceStatus == null)
            {
                return BadRequest("Unable to find the specified attendance status.");
            }
            try { 
                await _attendanceStatusRepo.Delete(attendanceStatus.Id);

                return Ok("Status deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("AllStudent")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetAllStudentAttendance()
        {
            IQueryable<StudentAttendance> query = await _studentAttendanceRepo.GetRelation();
            try
            {
                if (query.Any())
                {
                    var result = await query.Select(sa => new
                    {
                        sa.date,
                        courseId = sa.Course.Id,
                        sa.Course.Name,
                        sa.StudentID,
                        sa.Student.FirstName,
                        sa.Student.LastName,
                        sa.AttendanceStatus.Title
                    }).ToListAsync();
                    return Ok(result);
                }
                else
                {
                    return NotFound("No attendance records found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //GET url/Attendance/Student/get?dateTime=2024-05-24 | one date
        //Get url/Attendance/Student/get?startDate=2024-05-24&endDate=2024-11-01 | date to date
        //Get url/Attendance/Student/get?year=2024 | by year
        //Get url/Attendance/Student/get?month=05 | by month
        [HttpGet("StudentFilter")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetStudentAttendance(
            [FromQuery] int? coursId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] DateTime? dateTime
            )
        {
            try
            {
                IQueryable<StudentAttendance> query = await _studentAttendanceRepo.GetRelation();
                                                        
                if (coursId.HasValue)
                {
                    var course = await _coursRepo.GetById(coursId.Value);
                    if (course != null)
                    {
                        var start = course.StartDate;
                        var end = course.EndDate;
                        query = query.Where(ta => ta.date >= start && ta.date <= end);
                    }
                    else
                    {
                        return BadRequest($"No Course has this Id : {coursId.Value}");
                    }
                }
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(ta => ta.date >= startDate.Value && ta.date <= endDate.Value);
                }
                if (year.HasValue)
                {
                    query = query.Where(ta => ta.date.Year == year.Value);
                }
                if (month.HasValue)
                {
                    query = query.Where(ta => ta.date.Month == month.Value);
                }
                if (dateTime.HasValue)
                {
                    query = query.Where(sa => sa.date == dateTime);
                }
                if (query.Any())
                {
                    var result = await query.Select(sa => new 
                    {
                        sa.date,
                        courseId = sa.Course.Id,
                        sa.Course.Name,
                        sa.StudentID,
                        sa.Student.FirstName,
                        sa.Student.LastName,
                        sa.AttendanceStatus.Title
                    }).ToListAsync();
                    return Ok(result);
                }
                else
                {
                    return NotFound("No attendance records found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("AllTeacher")]
        public async Task<ActionResult<IEnumerable<TeacherAttendance>>> GetAllTeacherAttendance()
        {
            IQueryable<TeacherAttendance> query = await _teacherAttendanceRepo.GetRelation();
            try
            {
                if (query.Any())
                {
                    var result = await query.Select(sa => new
                    {
                        sa.date,
                        sa.CourseID,
                        sa.Course.Name,
                        TeacherId = sa.TeacherID,
                        TeacherFirstName = sa.Teacher.FirstName,
                        TeacherLastName = sa.Teacher.LastName,
                        sa.AttendanceStatus.Title
                    }).ToListAsync();
                    return Ok(result);
                }
                else
                {
                    return NotFound("No attendance records found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //GET url/Attendance/Teacher/get?dateTime=2024-05-24 | one date
        //Get url/Attendance/Teacher/get?startDate=2024-05-24&endDate=2024-11-01 | date to date
        //Get url/Attendance/Teacher/get?year=2024 | by year
        //Get url/Attendance/Teacher/get?month=05 | by month
        [HttpGet("TeacherFilter")]
        public async Task<ActionResult<IEnumerable<TeacherAttendance>>> GetTeacherAttendance(
            [FromQuery] int? coursId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] DateTime? dateTime
            )
        {
            try
            {
                IQueryable<TeacherAttendance> query = await _teacherAttendanceRepo.GetRelation();
                
                if (coursId.HasValue)
                {
                    var course = await _coursRepo.GetById(coursId.Value);
                    if (course != null)
                    {
                        var start = course.StartDate;
                        var end = course.EndDate;
                        query = query.Where(ta => ta.date >= start && ta.date <= end);
                    }
                    else
                    {
                        return BadRequest($"No Course has Id : {coursId.Value}");
                    }
                }
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(ta => ta.date >= startDate.Value && ta.date <= endDate.Value);
                }
                if (year.HasValue)
                {
                    query = query.Where(ta => ta.date.Year == year.Value);
                }
                if (month.HasValue)
                {
                    query = query.Where(ta => ta.date.Month == month.Value);
                }
                if (dateTime.HasValue)
                {
                    query = query.Where(sa => sa.date == dateTime);
                }
                if (query.Any())
                {
                    var result = await query.Select(sa => new
                    {
                        sa.date,
                        sa.CourseID,
                        sa.Course.Name,
                        TeacherId = sa.TeacherID,
                        TeacherFirstName = sa.Teacher.FirstName,
                        TeacherLastName = sa.Teacher.LastName,
                        sa.AttendanceStatus.Title
                    }).ToListAsync();
                    return Ok(result);
                }
                else
                {
                    return NotFound("No attendance records found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpPost("Student")]
        public async Task<IActionResult> AddStudentAttendance([FromForm] Map_StudentAttendance studentAttendance)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var attendanceTable = await _studentAttendanceRepo.GetRelation();
            if (await attendanceTable.AnyAsync(d => d.date == studentAttendance.Date))
            {
                return BadRequest("This date already exists.");
            }
            if (!await _coursRepo.IDExists(studentAttendance.CourseID ?? 0))
            {
                return BadRequest("Invalid Course ID.");
            }
            if (!await _studentRepo.IDExists(studentAttendance.StudentID ?? 0))
            {
                return BadRequest("Invalid Student ID.");
            }
            if (!await _attendanceStatusRepo.IDExists(studentAttendance.StatusID ?? 0))
            {
                return BadRequest("Invalid Status ID.");
            }
            var studentAttendanceResult =  _mapper.Map<StudentAttendance>(studentAttendance);
            try
            {
                await _studentAttendanceRepo.Add(studentAttendanceResult);
                return Ok("Attendance record added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }
        
        [HttpPost("AllStudentAttendance")]
        public async Task<IActionResult> AddAllStudentAttendance([FromForm] DateTime dateTime, [FromForm] int courseID,
            [FromForm] int statusID)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                if (!await _coursRepo.IDExists(courseID))
                {
                    return BadRequest("Invalid Course ID.");
                }
                if (!await _attendanceStatusRepo.IDExists(statusID))
                {
                    return BadRequest("Invalid Status ID.");
                }
                var attendanceTable = await _studentAttendanceRepo.GetRelation();
                var attendanceInDate = attendanceTable.Where(d => d.date == dateTime && d.CourseID == courseID);
                var studentIdsToUpdate = attendanceInDate.Select(a => a.StudentID).ToList();
                if (attendanceInDate.Any())
                {
                    foreach (var attendance in attendanceInDate)
                    {
                        attendance.StatusID = statusID;
                        _studentAttendanceRepo.ManyUpdate(attendance);
                    }
                }
                var allStudents = await _studentRepo.GetRelation();
                var studentsInCourse = allStudents.Where(s => s.CourseID == courseID && !studentIdsToUpdate.Contains(s.Id)).ToList();
                if ( !studentsInCourse.Any())
                {
                    return NotFound("No students found for the specified course.");
                }
                foreach (var student in studentsInCourse)
                {
                    StudentAttendance studentAttendance = new()
                    {
                        date = dateTime,
                        CourseID = courseID,
                        StudentID = student.Id,
                        StatusID = statusID
                    };
                    await _studentAttendanceRepo.ManyAdd(studentAttendance);
                }
                await _studentAttendanceRepo.SaveManyAdd();
                return Ok("Attendance records added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }
       
        [HttpPost("Teacher")]
        public async Task<IActionResult> AddTeacherAttendance([FromForm] Map_TeacherAttendance teacherAttendance)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var attendanceTable = await _teacherAttendanceRepo.GetRelation();
            if(await attendanceTable.AnyAsync(d => d.date == teacherAttendance.Date))
            {
                return BadRequest("This date already exists");
            }
            if (!await _coursRepo.IDExists(teacherAttendance.CourseID ?? 0))
            {
                return BadRequest("Invalid Course ID.");
            }
            if (!await _coursRepo.IsTeacherIDExists(teacherAttendance.TeacherID ?? ""))
            {
                return BadRequest("Invalid Teacher ID.");
            }
            if (!await _attendanceStatusRepo.IDExists(teacherAttendance.StatusID ?? 0))
            {
                return BadRequest("Invalid Status ID.");
            }
            var teacherAttendanceResult = _mapper.Map<TeacherAttendance>(teacherAttendance);
            try
            {
                await _teacherAttendanceRepo.Add(teacherAttendanceResult);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpPatch("Student")]
        public async Task<IActionResult> UpdateStudentAttendance([FromForm] Map_StudentAttendance studentAtt)
        {
            var allStudentAttendance = await _studentAttendanceRepo.GetRelation();
            var studentAttendance = await allStudentAttendance
                .Where(sa => 
                    sa.date == studentAtt.Date &&
                    sa.StudentID == studentAtt.StudentID &&
                    sa.CourseID == studentAtt.CourseID)
                .SingleOrDefaultAsync();

            if (studentAttendance == null)
            {
                return NotFound("Student Attendance not found.");
            }

            if (!await _attendanceStatusRepo.IDExists(studentAtt.StatusID ?? 0))
            {
                return BadRequest("Invalid Status ID.");
            }
            studentAttendance.StatusID = studentAtt.StatusID;
            try
            {
                await _studentAttendanceRepo.Update(studentAttendance);
                return Ok($"Attendance for the student with id:{studentAtt.StudentID} has changed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("Teacher")]
        public async Task<IActionResult> UpdateTeacherAttendance([FromForm] Map_TeacherAttendance teacherAtt)
        {
            var allTeacherAttendance = await _teacherAttendanceRepo.GetRelation();
            var teacherAttendance = await allTeacherAttendance
                .Where(ta =>
                        ta.date == teacherAtt.Date
                        && ta.TeacherID == teacherAtt.TeacherID 
                        && ta.CourseID == teacherAtt.CourseID)
                .SingleOrDefaultAsync();
            if (teacherAttendance == null)
            {
                return NotFound("Teacher Attendance not found.");
            }

            if (!await _attendanceStatusRepo.IDExists(teacherAtt.StatusID ?? 0))
            {
                return BadRequest("Invalid Status ID.");
            }
            teacherAttendance.StatusID = teacherAtt.StatusID;
            try
            {
                await _teacherAttendanceRepo.Update(teacherAttendance);
                return Ok($"Attendance changed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

