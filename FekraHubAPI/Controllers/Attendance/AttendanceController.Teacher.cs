using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Controllers.Attendance
{
    public partial class AttendanceController
    {
        [Authorize(Policy = "GetTeachersAttendance")]
        [HttpGet("TeacherAttendance/{Id}")]
        public async Task<ActionResult<IEnumerable<TeacherAttendance>>> GetTeacherAttendance(string Id)
        {
            try
            {
                IQueryable<TeacherAttendance> query = (await _teacherAttendanceRepo.GetRelation<TeacherAttendance>())
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
                DateTime? start = null;
                DateTime? end = null;
                List<string>? teacherIds = null;
                if (coursId.HasValue)
                {
                    var course = (await _coursRepo.GetRelation<Course>(x => x.Id == coursId.Value));
                    if (course == null)
                    {
                        return BadRequest($"No Course has Id : {coursId.Value}");
                    }
                    teacherIds = course.SelectMany(x => x.Teacher.Select(z => z.Id)).ToList();
                    if (teacherIds.Count == 0)
                    {
                        return BadRequest("No teacher records found in this course");
                    }
                    start = course.Single().StartDate;
                    end = course.Single().EndDate;

                }
                IQueryable<TeacherAttendance> query = (await _teacherAttendanceRepo.GetRelation<TeacherAttendance>(null,
                    new List<Expression<Func<TeacherAttendance, bool>>?>
                    {
                        coursId.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date >= start && ta.date <= end && teacherIds.Contains(ta.TeacherID ?? "")) : null,
                        startDate.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date >= startDate.Value) : null,
                        endDate.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date <= endDate.Value) : null,
                        year.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date.Year == year.Value) : null,
                        month.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date.Month == month.Value) : null,
                        dateTime.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(sa => sa.date.Date == dateTime.Value.Date) : null,
                    }.Where(x => x != null).Cast<Expression<Func<TeacherAttendance, bool>>>().ToList()
                    )).OrderByDescending(ta => ta.date);

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
                var workingD = await _courseScheduleRepo.GetRelation<CourseSchedule>();
                var courses = await _coursRepo.GetRelation<Course>();
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
                var CourseAttExist = await _courseAttendanceRepo.GetRelation<CourseAttendance>();

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
                var techerAtten = (await _teacherAttendanceRepo.GetRelation<TeacherAttendance>())
                    .Any(x => x.date.Date == DateTime.Now.Date && teacherAttendance.Select(z => z.TeacherID).ToList().Contains(x.TeacherID));
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
            var allTeacherAttendance = await _teacherAttendanceRepo.GetRelation<TeacherAttendance>();
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
