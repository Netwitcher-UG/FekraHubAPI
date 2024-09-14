using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Constract;
using FekraHubAPI.Repositories.Interfaces;

namespace FekraHubAPI.Controllers.Attendance
{
    public partial class AttendanceController
    {
        [Authorize]
        [HttpGet("TeachersName")]
        public async Task<IActionResult> GetTeachers()
        {
            var x = await _userManager.GetUsersInRoleAsync("Teacher");
            return Ok(x.Select(x => new { x.Id, x.FirstName, x.LastName }));
        }

        [Authorize(Policy = "GetTeachersAttendance")]
        [HttpGet("TeacherAttendance/{Id}")]
        public async Task<ActionResult<IEnumerable<TeacherAttendance>>> GetTeacherAttendance(string Id)
        {
            try
            {
                var result = await _teacherAttendanceRepo.GetRelationList(
                    where: x => x.TeacherID == Id,
                    include:x=>x.Include(z=>z.Course).Include(t => t.Teacher).Include(at => at.AttendanceStatus),
                    orderBy: x => x.date,
                    selector: sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        Teacher = new { sa.Teacher.Id, sa.Teacher.FirstName, sa.Teacher.LastName },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title },
                    },
                    asNoTracking: true);
                if (result.Any())
                {
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
        [Authorize(Policy = "GetTeacher")]
        [HttpGet("TeacherAttendanceProfile")]
        public async Task<ActionResult<IEnumerable<TeacherAttendance>>> GetTeacherAttendanceProfile(string id)
        {
            try
            {
                var Teacher = await _userManager.FindByIdAsync(id);
                if (Teacher == null)
                {
                    return BadRequest("Teacher not found");
                }
                var isTeacher = await _teacherAttendanceRepo.IsTeacherIDExists(id);
                if (!isTeacher)
                {
                    return BadRequest("The Id does not belong to a teacher");
                }
                var teacherAttendance = await _teacherAttendanceRepo.GetRelationList(
                    where: x => x.TeacherID == id,
                    include: x => x.Include(z => z.Course).Include(t => t.Teacher).Include(at => at.AttendanceStatus),
                    orderBy: x => x.date,
                    selector: sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title },
                    },
                    asNoTracking: true);
                
                return Ok(new { Teacher = new { Teacher.Id, Teacher.FirstName, Teacher.LastName }, teacherAttendance });
                
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
               
              
                var result = await _teacherAttendanceRepo.GetRelationList(
                    manyWhere: new List<Expression<Func<TeacherAttendance, bool>>?>
                    {
                        teacherId != null ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.TeacherID == teacherId) : null,
                        coursId.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.CourseID == coursId) : null,
                        startDate.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date >= startDate.Value) : null,
                        endDate.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date <= endDate.Value) : null,
                        year.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date.Year == year.Value) : null,
                        month.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date.Month == month.Value) : null,
                        dateTime.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(sa => sa.date.Date == dateTime.Value.Date) : null,
                    }.Where(x => x != null).Cast<Expression<Func<TeacherAttendance, bool>>>().ToList(),
                    orderBy: ta => ta.date,
                    include: x => x.Include(z => z.Course).Include(t => t.Teacher).Include(at => at.AttendanceStatus),
                    selector: sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        Teacher = new { sa.Teacher.Id, sa.Teacher.FirstName, sa.Teacher.LastName },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title }
                    },
                    asNoTracking: true);

                if (result.Any())
                {
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

                var existingDate = await _attendanceDateRepo.DataExist(x => x.Date.Date == teacherAttendance.Date.Date);

                if (!existingDate)
                {
                    return BadRequest("This date not a working day");
                }

                //course teacher
                var couseId = await _coursRepo.GetRelationSingle(
                    where: x => x.Teacher.Select(z => z.Id).Contains(teacherAttendance.TeacherID),
                    selector: x => x.Id,
                    returnType: QueryReturnType.FirstOrDefault,
                    asNoTracking:true);
                   

                if (couseId == 0)
                {
                    return BadRequest($"The teacher with the Id {teacherAttendance.TeacherID} is not belong to the course");
                }

                // from course schedule (working days)
                var workingDays = await _courseScheduleRepo.GetRelationList(
                    where:x => x.CourseID == couseId,
                    selector: x => x.DayOfWeek.ToLower());
                if (!workingDays.Any())
                {
                    return NotFound("Course working days are not recorded in the school system");
                }
                if (!workingDays.Contains(teacherAttendance.Date.DayOfWeek.ToString().ToLower()))
                {
                    return BadRequest($"Date was not registered as a working day for this course with the id {couseId}");
                }

                var techerAtten = await _teacherAttendanceRepo.DataExist(
                    x => x.date.Date == teacherAttendance.Date.Date && x.TeacherID == teacherAttendance.TeacherID);

                if (techerAtten)
                {
                    return BadRequest("The teachers already have attendance");
                }
                var tAttendance = new TeacherAttendance
                {
                    date = teacherAttendance.Date,
                    CourseID = couseId,
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
        public async Task<IActionResult> UpdateTeacherAttendance( int id,  int statusId)
        {
            
            try
            {
                var teacherAttendance = await _teacherAttendanceRepo.GetById(id);
               
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

                var TeacherAtt = await _teacherAttendanceRepo.GetById(id);
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
