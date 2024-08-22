using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [Authorize(Policy = "GetStudentsAttendance")]
        [HttpGet("StudentAttendance/{Id}")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetStudentAttendance(int Id)
        {

            try
            {
                IQueryable<StudentAttendance> query = (await _studentAttendanceRepo.GetRelation())
                                                    .Where(x => x.StudentID == Id)
                                                    .OrderByDescending(x => x.date);
                if (query.Any())
                {
                    var result = await query.Select(sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        student = new { sa.Student.Id, sa.Student.FirstName, sa.Student.LastName },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title },
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

        [Authorize(Policy = "GetStudentsAttendance")]
        [HttpGet("StudentFilter")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetStudentAttendance(
            [FromQuery] int coursId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] DateTime? dateTime
            )
        {
            try
            {
                var course = await _coursRepo.GetById(coursId);
                if (course == null)
                {
                    return BadRequest($"No Course found");

                }
                IQueryable<StudentAttendance> query = (await _studentAttendanceRepo.GetRelation()).Where(x => x.CourseID == coursId);
                var start = course.StartDate;
                var end = course.EndDate;
                query = query.Where(ta => ta.date >= start && ta.date <= end).OrderByDescending(d => d.date);

                if (startDate.HasValue)
                {
                    query = query.Where(ta => ta.date >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(ta => ta.date <= endDate.Value);
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
                    query = query.Where(sa => sa.date.Date == dateTime.Value.Date);
                }
                if (query.Any())
                {
                    var result = await query.Select(sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        student = new { sa.Student.Id, sa.Student.FirstName, sa.Student.LastName },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title }
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

        [Authorize(Policy = "AddStudentAttendance")]
        [HttpPost("Student")]
        public async Task<IActionResult> AddStudentAttendance(
            List<Map_StudentAttendance>? studentAttendance,
             int courseId
            )
        {
            try
            {
                // course exist or not
                var course = (await _coursRepo.GetRelation()).Where(x => x.Id == courseId);
                if (!course.Any())
                {
                    return BadRequest("Course not found");
                }
                // from course schedule (working days)
                var workingDays = (await _courseScheduleRepo.GetRelation())
                                                            .Where(x => x.CourseID == courseId)
                                                            .Select(x => x.DayOfWeek.ToLower())
                                                            .ToList();
                if (!workingDays.Any())
                {
                    return NotFound("Course working days are not recorded in the school system");
                }
                if (!workingDays.Contains(DateTime.Now.DayOfWeek.ToString().ToLower()))
                {
                    return BadRequest("Today was not registered as a working day for this course");
                }
                // student ids are in this course or not
                if (studentAttendance != null && studentAttendance.Any())
                {
                    var studentsIdInCourse = (await _studentRepo.GetAll())
                        .Where(x => x.CourseID == courseId)
                        .Select(x => x.Id)
                        .ToList();

                    var studentsInList = studentAttendance
                        .Where(sa => !studentsIdInCourse.Contains(sa.StudentID))
                        .Select(sa => sa.StudentID)
                        .ToList();

                    if (studentsInList.Any())
                    {
                        return BadRequest("Some students do not belong to the course.");
                    }
                }

                // date now if exist in database or not  (add if not)
                var today = DateTime.Now.Date;
                var existingDate = (await _attendanceDateRepo.GetRelation())
                                    .Where(x => x.Date.Date == today)
                                    .FirstOrDefault();
                var attDateId = 0;
                if (existingDate == null)
                {
                    var newAttendanceDate = new AttendanceDate
                    {
                        Date = today
                    };

                    await _attendanceDateRepo.Add(newAttendanceDate);
                    attDateId = newAttendanceDate.Id;
                }
                else
                {
                    attDateId = existingDate.Id;
                }
                // course added attendance for today or not
                var CourseAttExist = await _courseAttendanceRepo.GetRelation();
                if (CourseAttExist.Any())
                {
                    var IsCourseAttExist = course.SelectMany(z => z.CourseAttendance)
                                        .Any(ca => ca.CourseId == courseId && ca.AttendanceDateId == existingDate.Id);
                    if (IsCourseAttExist)
                    {
                        return BadRequest("This course has an attendance today");
                    }
                }
                // finaly add student and course attendance
                var newAttendance = new List<StudentAttendance>();
                if (studentAttendance.Any())
                {
                    foreach (var studentAtt in studentAttendance)
                    {
                        newAttendance.Add(
                            new StudentAttendance
                            {
                                date = today,
                                CourseID = courseId,
                                StudentID = studentAtt.StudentID,
                                StatusID = studentAtt.StatusID
                            });
                    }
                }
                await _studentAttendanceRepo.ManyAdd(newAttendance);
                await _courseAttendanceRepo.Add(new CourseAttendance
                {
                    CourseId = courseId,
                    AttendanceDateId = attDateId
                });



                return Ok(newAttendance.Select(x => new { x.Id, x.StudentID, x.CourseID, x.date, x.StatusID }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Policy = "UpdateStudentsAttendance")]

        [HttpPatch("Student")]
        public async Task<IActionResult> UpdateStudentAttendance([FromForm] int id, [FromForm] int statusId)
        {
            var allStudentAttendance = await _studentAttendanceRepo.GetRelation();
            var studentAttendance = await allStudentAttendance
                .Where(sa => sa.Id == id)
                .SingleOrDefaultAsync();

            if (studentAttendance == null)
            {
                return NotFound("Student Attendance not found.");
            }

            studentAttendance.StatusID = statusId;
            try
            {
                await _studentAttendanceRepo.Update(studentAttendance);
                return Ok(new
                {
                    studentAttendance.Id,
                    studentAttendance.date,
                    studentAttendance.CourseID,
                    studentAttendance.StudentID,
                    NewStatusId = studentAttendance.StatusID
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Policy = "UpdateStudentsAttendance")]
        [HttpDelete("Student")]
        public async Task<IActionResult> DeleteStudentAttendance([FromForm] int id)
        {
            try
            {
                var allStudentAttendance = await _studentAttendanceRepo.GetRelation();
                var studentAttendance = await allStudentAttendance
                    .Where(sa => sa.Id == id)
                    .SingleOrDefaultAsync();

                if (studentAttendance == null)
                {
                    return NotFound("Student Attendance not found.");
                }

            
                    await _studentAttendanceRepo.Delete(id);
                    return Ok("Delete success");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [Authorize(Policy = "GetTeachersAttendance")]
        [HttpGet("TeacherAttendance/{Id}")]
        public async Task<ActionResult<IEnumerable<TeacherAttendance>>> GetTeacherAttendance(string Id)
        {
            try
            {
                IQueryable<TeacherAttendance> query = (await _teacherAttendanceRepo.GetRelation())
                                                    .Where(x => x.TeacherID == Id)
                                                    .OrderByDescending(x => x.date);
                if (query.Any())
                {
                    var result = await query.Select(sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        Teacher = new { sa.Teacher.Id, sa.Teacher.FirstName, sa.Teacher.LastName },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title },
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

        [Authorize(Policy = "GetTeachersAttendance")]
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
                    if (course == null)
                    {
                        return BadRequest($"No Course has Id : {coursId.Value}");
                    }
                    var teacherIds = course.Teacher.Select(x => x.Id).ToList();
                    if (teacherIds.Count == 0)
                    {
                        return BadRequest("No teacher records found in this course");
                    }
                    var start = course.StartDate;
                    var end = course.EndDate;
                    query = query.Where(ta => ta.date >= start && ta.date <= end && teacherIds.Contains(ta.TeacherID ?? ""));

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
                query = query.OrderByDescending(ta => ta.date);
                if (query.Any())
                {
                    var result = await query.Select(sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        Teacher = new { sa.Teacher.Id, sa.Teacher.FirstName, sa.Teacher.LastName },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title }
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

        [Authorize(Policy = "UpdateTeachersAttendance")]
        [HttpPost("Teacher")]
        public async Task<IActionResult> AddTeacherAttendance(List<Map_TeacherAttendance> teacherAttendance)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!teacherAttendance.Any())
                {
                    return NotFound();
                }
                var workingD = await _courseScheduleRepo.GetRelation();
                var courses = await _coursRepo.GetRelation();
                // date now if exist in database or not  (add if not)
                var today = DateTime.Now.Date;
                var existingDate = (await _attendanceDateRepo.GetRelation())
                                    .Where(x => x.Date.Date == today)
                                    .FirstOrDefault();
                var attDateId = 0;
                if (existingDate == null)
                {
                    var newAttendanceDate = new AttendanceDate
                    {
                        Date = today
                    };

                    await _attendanceDateRepo.Add(newAttendanceDate);
                    attDateId = newAttendanceDate.Id;
                }
                else
                {
                    attDateId = existingDate.Id;
                }
                var CourseAttExist = await _courseAttendanceRepo.GetRelation();
                
                foreach (var teacherAtt in teacherAttendance)
                {
                    // course exist or not
                    var course = courses.Where(x => x.Id == teacherAtt.CourseID);
                    if (!course.Any())
                    {
                        return BadRequest($"The teacher with the Id {teacherAtt.TeacherID} is not registered in a course");
                    }

                    // from course schedule (working days)
                    var workingDays = workingD.Where(x => x.CourseID == teacherAtt.CourseID)
                                              .Select(x => x.DayOfWeek.ToLower())
                                              .ToList();
                    if (!workingDays.Any())
                    {
                        return NotFound("Course working days are not recorded in the school system");
                    }
                    if (!workingDays.Contains(DateTime.Now.DayOfWeek.ToString().ToLower()))
                    {
                        return BadRequest($"Today was not registered as a working day for this course with the id {teacherAtt.CourseID}");
                    }
                    // teacher ids are in this course or not
                    var teacherIds = course.SelectMany(x => x.Teacher.Select(z => z.Id)).ToList();
                    if (teacherIds.Contains(teacherAtt.TeacherID))
                    {
                        return BadRequest($"The teacher with the Id {teacherAtt.TeacherID} is not belong to the course with the id {teacherAtt.CourseID}");
                    }
                }
                var techerAtten = (await _teacherAttendanceRepo.GetRelation())
                    .Any(x =>x.date.Date == DateTime.Now.Date && teacherAttendance.Select(z => z.TeacherID).ToList().Contains(x.TeacherID));
                if (techerAtten)
                {
                    return BadRequest("One or more of the teachers already have attendance");
                }
                List<TeacherAttendance> tAttendance = new();

                foreach (var techAttendance in teacherAttendance)
                {
                    tAttendance.Add(new TeacherAttendance
                    {
                        date = today,
                        CourseID = techAttendance.CourseID,
                        TeacherID = techAttendance.TeacherID,
                        StatusID = techAttendance.StatusID
                    });
                }
                await _teacherAttendanceRepo.ManyAdd(tAttendance);

                return Ok(tAttendance.Select(Result => new { Result.Id, Result.TeacherID, Result.CourseID, Result.date, Result.StatusID }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [Authorize(Policy = "UpdateTeachersAttendance")]
        [HttpPatch("Teacher")]
        public async Task<IActionResult> UpdateTeacherAttendance([FromForm] int id, [FromForm] int statusId)
        {
            var allTeacherAttendance = await _teacherAttendanceRepo.GetRelation();
            var teacherAttendance = await allTeacherAttendance
                .Where(sa => sa.Id == id)
                .SingleOrDefaultAsync();
            if (teacherAttendance == null)
            {
                return NotFound("Teacher Attendance not found.");
            }
            teacherAttendance.StatusID = statusId;
            try
            {
                await _teacherAttendanceRepo.Update(teacherAttendance);
                return Ok(new
                {
                    teacherAttendance.Id,
                    teacherAttendance.date,
                    teacherAttendance.CourseID,
                    teacherAttendance.TeacherID,
                    NewStatusId = teacherAttendance.StatusID
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}

