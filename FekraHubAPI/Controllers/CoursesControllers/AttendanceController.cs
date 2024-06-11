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

        [HttpDelete("AttendanceStatus/{id}")]
        public async Task<IActionResult> DeleteAttendanceStatus( int id)
        {
            
            var attendanceStatus = await _attendanceStatusRepo.GetById(id);
            if (attendanceStatus == null)
            {
                return NotFound("The status with the provided ID does not exist.");
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
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        student = new { sa.Student.Id, sa.Student.FirstName , sa.Student.LastName },
                        AttendanceStatus = new {sa.AttendanceStatus.Id,sa.AttendanceStatus.Title}
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
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            
            var studentAttendanceResult =  _mapper.Map<StudentAttendance>(studentAttendance);
            try
            {
                await _studentAttendanceRepo.Add(studentAttendanceResult);
                return Ok("Attendance record added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
                var attendanceInDate = (await _studentAttendanceRepo.GetRelation()).Where(d => d.date == dateTime && d.CourseID == courseID);
                var studentIdsToUpdate = attendanceInDate.Select(a => a.StudentID).ToList();
                if (attendanceInDate.Any())
                {
                    await attendanceInDate.ForEachAsync(attendance => attendance.StatusID = statusID);
                    await _studentAttendanceRepo.ManyUpdate(attendanceInDate);
                }
                var allStudents = await _studentRepo.GetRelation();
                var studentsInCourse = allStudents.Where(s => s.CourseID == courseID && !studentIdsToUpdate.Contains(s.Id)).ToList();
                if ( !studentsInCourse.Any())
                {
                    return NotFound("No students found for the specified course.");
                }
               
                List<StudentAttendance> studentAttendances = studentsInCourse
                        .Select(student => new StudentAttendance
                        {
                            date = dateTime,
                            CourseID = courseID,
                            StudentID = student.Id,
                            StatusID = statusID
                        }).ToList();
                await _studentAttendanceRepo.ManyAdd(studentAttendances);
                return Ok("Attendance records added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            if (await attendanceTable.AnyAsync(d => d.date == teacherAttendance.Date))
            {
                return BadRequest("This date already exists");
            }
            var teacherAttendanceResult = _mapper.Map<TeacherAttendance>(teacherAttendance);
            try
            {
                await _teacherAttendanceRepo.Add(teacherAttendanceResult);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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

            studentAttendance.StatusID = studentAtt.StatusID;
            try
            {
                await _studentAttendanceRepo.Update(studentAttendance);
                return Ok($"Attendance for the student with id:{studentAtt.StudentID} has changed");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            teacherAttendance.StatusID = teacherAtt.StatusID;
            try
            {
                await _teacherAttendanceRepo.Update(teacherAttendance);
                return Ok($"Attendance changed");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}

