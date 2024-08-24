using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Controllers.Attendance
{
    public partial class AttendanceController
    {
        [Authorize(Policy = "GetStudentsAttendance")]
        [HttpGet("StudentAttendance/{Id}")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetStudentAttendance(int Id)
        {

            try
            {
                IQueryable<StudentAttendance> query = (await _studentAttendanceRepo.GetRelation<StudentAttendance>(
                                                    x => x.StudentID == Id))
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
                IQueryable<StudentAttendance> query = (await _studentAttendanceRepo.GetRelation<StudentAttendance>(null,
                    new List<Expression<Func<StudentAttendance, bool>>?>
                    {
                        x => x.CourseID == coursId,
                        ta => ta.date >= course.StartDate && ta.date <= course.EndDate,
                        startDate.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date >= startDate.Value) : null,
                        endDate.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date <= endDate.Value) : null,
                        year.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date.Year == year.Value) : null,
                        month.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date.Month == month.Value) : null,
                        dateTime.HasValue ? (Expression<Func<StudentAttendance, bool>>)(sa => sa.date.Date == dateTime.Value.Date) : null,
                    }.Where(x => x != null).Cast<Expression<Func<StudentAttendance, bool>>>().ToList()
                    )).OrderByDescending(ta => ta.date);

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
                var course = (await _coursRepo.GetRelation<Course>()).Where(x => x.Id == courseId);
                if (!course.Any())
                {
                    return BadRequest("Course not found");
                }
                // from course schedule (working days)
                var workingDays = (await _courseScheduleRepo.GetRelation<CourseSchedule>())
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
                var existingDate = (await _attendanceDateRepo.GetRelation<AttendanceDate>())
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
                var CourseAttExist = await _courseAttendanceRepo.GetRelation<CourseAttendance>();
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
            var allStudentAttendance = await _studentAttendanceRepo.GetRelation<StudentAttendance>();
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
                var allStudentAttendance = await _studentAttendanceRepo.GetRelation<StudentAttendance>();
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
    }
}
