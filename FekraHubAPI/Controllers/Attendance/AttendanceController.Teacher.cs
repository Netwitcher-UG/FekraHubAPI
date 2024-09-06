using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Constract;

namespace FekraHubAPI.Controllers.Attendance
{
    public partial class AttendanceController
    {
        [Authorize]
        [HttpGet("TeachersName")]
        public async Task<IActionResult> GetTeachers()
        {
            var x = await _userManager.GetUsersInRoleAsync("Teacher");
            return Ok(x.Select(x => new { x.Id,x.FirstName,x.LastName }));
        }

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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Policy = "GetTeachersAttendance")]
        [HttpGet("TeacherFilter")]
        public async Task<ActionResult<IEnumerable<TeacherAttendance>>> GetTeacherAttendance(
            [FromQuery] string? teacherId,
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
               
              
                IQueryable<TeacherAttendance> query = (await _teacherAttendanceRepo.GetRelation<TeacherAttendance>(null,
                    new List<Expression<Func<TeacherAttendance, bool>>?>
                    {
                        teacherId != null ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.TeacherID == teacherId) : null,
                        coursId.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.CourseID == coursId) : null,
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "UpdateTeachersAttendance")]
        [HttpPost("Teacher")]
        public async Task<IActionResult> AddTeacherAttendance(Map_TeacherAttendance teacherAttendance)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var workingD = await _courseScheduleRepo.GetRelation<CourseSchedule>();
                
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

                // course exist or not
                var courses = await _coursRepo.GetRelation<Course>(x => x.Id == teacherAttendance.CourseID);
                var course = courses.SingleOrDefault();
                if (course == null)
                {
                    return BadRequest($"The teacher with the Id {teacherAttendance.TeacherID} is not registered in a course");
                }
                // teacher id are in this course or not
                var teacherIds = courses.Any(z => z.Teacher.Select(z => z.Id).Contains(teacherAttendance.TeacherID));

                if (!teacherIds)
                {
                    return BadRequest($"The teacher with the Id {teacherAttendance.TeacherID} is not belong to the course with the id {teacherAttendance.CourseID}");
                }
                // from course schedule (working days)
                var workingDays = workingD.Where(x => x.CourseID == teacherAttendance.CourseID)
                                          .Select(x => x.DayOfWeek.ToLower())
                                          .ToList();
                if (!workingDays.Any())
                {
                    return NotFound("Course working days are not recorded in the school system");
                }
                if (!workingDays.Contains(DateTime.Now.DayOfWeek.ToString().ToLower()))
                {
                    return BadRequest($"Today was not registered as a working day for this course with the id {teacherAttendance.CourseID}");
                }
                
                var techerAtten = (await _teacherAttendanceRepo.GetRelation<TeacherAttendance>(
                    x => x.date.Date == DateTime.Now.Date && x.TeacherID == teacherAttendance.TeacherID)).ToList();
                  
                if (techerAtten.Any())
                {
                    return BadRequest("The teachers already have attendance");
                }
                var tAttendance = new TeacherAttendance
                {
                    date = today,
                    CourseID = teacherAttendance.CourseID,
                    TeacherID = teacherAttendance.TeacherID,
                    StatusID = teacherAttendance.StatusID
                };



                await _teacherAttendanceRepo.Add(tAttendance);

                return Ok(new { tAttendance.Id, tAttendance.TeacherID, tAttendance.CourseID, tAttendance.date, tAttendance.StatusID });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Policy = "UpdateTeachersAttendance")]
        [HttpPatch("Teacher")]
        public async Task<IActionResult> UpdateTeacherAttendance([FromForm] int id, [FromForm] int statusId)
        {
            
            try
            {
                var teacherAttendance = (await _teacherAttendanceRepo.GetRelation<TeacherAttendance>(sa => sa.Id == id))
                    .SingleOrDefault();
               
                if (teacherAttendance == null)
                {
                    return NotFound("Teacher Attendance not found.");
                }
                teacherAttendance.StatusID = statusId;
                await _teacherAttendanceRepo.Update(teacherAttendance);
                return Ok(new
                {
                    teacherAttendance.Id,
                    teacherAttendance.date,
                    teacherAttendance.CourseID,
                    teacherAttendance.TeacherID,
                    teacherAttendance.StatusID
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "UpdateTeachersAttendance")]
        [HttpDelete("DeleteTeacherAttendance")]
        public async Task<IActionResult> DeleteTeacherAttendance(int id)
        {
            try
            {

                var TeacherAtt = (await _teacherAttendanceRepo.GetRelation<TeacherAttendance>(sa => sa.Id == id))
                    .SingleOrDefault();
                if (TeacherAtt == null)
                {
                    return NotFound("Teacher Attendance not found.");
                }
                await _teacherAttendanceRepo.Delete(id);
                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
    }
}
