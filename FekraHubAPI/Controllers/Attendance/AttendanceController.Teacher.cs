using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Helpers;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

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
             include: x => x.Include(z => z.Course).Include(t => t.Teacher).Include(at => at.AttendanceStatus),
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

                return Ok(result) ;
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
                    return BadRequest("Lehrer nicht gefunden.");
                }

                var isTeacher = await _teacherAttendanceRepo.IsTeacherIDExists(id);
                if (!isTeacher)
                {
                    return BadRequest("Die ID gehört nicht zu einem Lehrer.");
                }

                var teacherAttendance = await _teacherAttendanceRepo.GetRelationList(
                    where: x => x.TeacherID == id,
                    include: x => x.Include(z => z.Course).Include(at => at.AttendanceStatus),
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

                var startUtc = startDate?.ToUtcSafe();
                var endUtc = endDate?.ToUtcSafe();

                DateTime? dateOnlyUtc = null;
                if (dateTime.HasValue)
                {
                    dateOnlyUtc = dateTime.Value.ToUtcSafe().Date;
                }
                var result = await _teacherAttendanceRepo.GetRelationList(
            manyWhere: new List<Expression<Func<TeacherAttendance, bool>>?>
            {
                teacherId != null ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.TeacherID == teacherId) : null,
                coursId.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.CourseID == coursId) : null,
                startUtc.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date >= startUtc.Value) : null,
                endUtc.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date <= endUtc.Value) : null,
                year.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date.Year == year.Value) : null,
                month.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(ta => ta.date.Month == month.Value) : null,
                dateOnlyUtc.HasValue ? (Expression<Func<TeacherAttendance, bool>>)(sa => sa.date.Date == dateOnlyUtc.Value) : null,
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

                return result.Any() ? Ok(result) : BadRequest("Keine Anwesenheitsaufzeichnungen gefunden.");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        //[Authorize(Policy = "UpdateTeachersAttendance")]
        [HttpPost("Teacher")]
        public async Task<IActionResult> AddTeacherAttendance([FromForm]Map_TeacherAttendance teacherAttendance)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                //var existingDate = await _attendanceDateRepo.DataExist(x => x.Date.Date == teacherAttendance.Date.Date.ToUtcSafe());

                //if (!existingDate)
                //{
                //    return BadRequest("Dieses Datum ist kein Arbeitstag.");//This date not a working day
                //}

                //course teacher
                var courseIds = await _coursRepo.GetRelationList(
                    where: x => x.Teacher.Select(z => z.Id).Contains(teacherAttendance.TeacherID),
                    selector: x => x.Id,
                    asNoTracking:true);


                if (!courseIds.Any())
                {
                    return BadRequest("Der Lehrer gehört nicht zu einem Kurs."); // المدرّس لا يخص أي كورس
                }

                var workingDays = await _courseScheduleRepo.GetRelationList(
                    where: x => courseIds.Contains(x.CourseID??0),
                    selector: x => new { x.CourseID, Day = x.DayOfWeek.ToLower() });

                if (!workingDays.Any())
                {
                    return BadRequest("Die Arbeitstage der Kurse sind nicht im Schulsystem verzeichnet.");
                }

                var dayOfWeek = teacherAttendance.Date.DayOfWeek.ToString().ToLower();

                var selectedCourseId = workingDays
                    .Where(w => w.Day == dayOfWeek)
                    .Select(w => w.CourseID)
                    .FirstOrDefault();        

                if (selectedCourseId == 0)
                {
                    return BadRequest("Das Datum wurde nicht als Arbeitstag für einen Kurs des Lehrers registriert.");
                }


                var dateUtc = teacherAttendance.Date.ToUtcSafe();
                var dateOnly = dateUtc.Date;

                var teacherHasAttendance = await _teacherAttendanceRepo.DataExist(
                            x => x.date.Date == dateOnly
                         && x.TeacherID == teacherAttendance.TeacherID);

                if (teacherHasAttendance)
                {
                    return BadRequest("Die Lehrer haben bereits eine Anwesenheit an diesem Datum.");// المدرّس لديه حضور بالفعل في هذا التاريخ
                }

                var tAttendance = new TeacherAttendance
                {
                    date = dateUtc,
                    CourseID = selectedCourseId,         
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
                    return BadRequest("Lehreranwesenheit nicht gefunden.");//Teacher Attendance not found.
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
                    return BadRequest("Lehreranwesenheit nicht gefunden.");//Teacher Attendance not found.
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
