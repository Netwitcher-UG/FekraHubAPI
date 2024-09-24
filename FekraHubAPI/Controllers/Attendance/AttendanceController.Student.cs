﻿using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Constract;
using System.ComponentModel.DataAnnotations;
using FekraHubAPI.Repositories.Interfaces;

namespace FekraHubAPI.Controllers.Attendance
{
    public partial class AttendanceController
    {
        [Authorize(Policy = "ManageChildren")]
        [HttpGet("StudentAttendanceForParent/{Id}")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetStudentAttendanceForParent(int Id)
        {

            try
            {
                var student = await _studentRepo.GetRelationSingle(
                    where: x => x.Id == Id,
                    selector: x => x,
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking:true);
                if (student == null)
                {
                    return BadRequest("Student not found");
                }
                string userId = _studentAttendanceRepo.GetUserIDFromToken(User);

                if (student.ParentID != userId)
                {
                    return BadRequest("This student is not your child");
                }
                var result = await _studentAttendanceRepo.GetRelationList(
                       where: x => x.StudentID == Id,
                       include: q => q.Include(x => x.Course).Include(x => x.Student).Include(x => x.AttendanceStatus),
                       orderBy: x => x.date,
                       selector: sa => new
                       {
                           id = sa.Id,
                           Date = sa.date,
                           course = new { sa.Course.Id, sa.Course.Name },
                           student = new { sa.Student.Id, sa.Student.FirstName, sa.Student.LastName },
                           AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title },
                       },
                       asNoTracking:true);
                
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

        [Authorize(Policy = "GetStudentsAttendance")]
        [HttpGet("StudentAttendance/{Id}")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetStudentAttendance(int Id)
        {

            try
            {
                var student = await _studentRepo.GetRelationSingle(
                    where: x => x.Id == Id,
                    selector: x => x,
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking: true);
                if (student == null)
                {
                    return BadRequest("Student not found");
                }
                string userId = _studentAttendanceRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentAttendanceRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    var course = await _coursRepo.GetRelationList(
                        where: x =>x.Teacher.Select(x=> x.Id).Contains(userId) && x.Id == student.CourseID,
                        selector: x => x,
                        asNoTracking: true);
                    if (!course.Any())
                    {
                        return BadRequest("This student is not in your course");
                    }
                   
                }
                var results = await _studentAttendanceRepo.GetRelationList(
                    where: x => x.StudentID == Id,
                    include: q => q.Include(x => x.Course).Include(x => x.Student).Include(x => x.AttendanceStatus),
                    orderBy: x => x.date,
                    selector: sa => new
                    {
                        id = sa.Id,
                        Date = sa.date,
                        course = new { sa.Course.Id, sa.Course.Name },
                        student = new { sa.Student.Id, sa.Student.FirstName, sa.Student.LastName },
                        AttendanceStatus = new { sa.AttendanceStatus.Id, sa.AttendanceStatus.Title }
                    },
                    asNoTracking: true);
                                                    
                if (results.Any())
                {
                    
                    return Ok(results);
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

        [Authorize(Policy = "GetStudentsAttendance")]
        [HttpGet("StudentFilter")]
        public async Task<ActionResult<IEnumerable<StudentAttendance>>> GetStudentAttendance(
            [FromQuery] int courseId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] DateTime? dateTime
            )
        {
            try
            {
                
                var course = await _coursRepo.GetById(courseId);
                if (course == null)
                {
                    return BadRequest($"No Course found");

                }
                string userId = _studentAttendanceRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentAttendanceRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    var teacherCourse = await _coursRepo.GetRelationSingle(
                        where: x => x.Id == courseId && x.Teacher.Select(x => x.Id).Contains(userId),
                    selector: x => x,
                    returnType: QueryReturnType.FirstOrDefault,
                    asNoTracking: true);
                    if (teacherCourse == null)
                    {
                        return BadRequest("This is not your course");
                    }

                }
                var result = await _studentAttendanceRepo.GetRelationList(
                   manyWhere: new List<Expression<Func<StudentAttendance, bool>>?>
                    {
                        x => x.CourseID == courseId,
                        ta => ta.date >= course.StartDate && ta.date <= course.EndDate,
                        startDate.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date >= startDate.Value) : null,
                        endDate.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date <= endDate.Value) : null,
                        year.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date.Year == year.Value) : null,
                        month.HasValue ? (Expression<Func<StudentAttendance, bool>>)(ta => ta.date.Month == month.Value) : null,
                        dateTime.HasValue ? (Expression<Func<StudentAttendance, bool>>)(sa => sa.date.Date == dateTime.Value.Date) : null,
                    }.Where(x => x != null).Cast<Expression<Func<StudentAttendance, bool>>>().ToList(),
                   orderBy: ta => ta.date,
                   include: q => q.Include(x => x.Course).Include(x => x.Student).Include(x => x.AttendanceStatus),
                   selector: sa => new
                   {
                       id = sa.Id,
                       Date = sa.date,
                       course = new { sa.Course.Id, sa.Course.Name },
                       student = new { sa.Student.Id, sa.Student.FirstName, sa.Student.LastName },
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
                var course = await _coursRepo.GetRelationSingle(
                    where: x => x.Id == courseId,
                    selector: x => x,
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);
                if (course == null)
                {
                    return BadRequest("Course not found");
                }
                // for teacher
                string userId = _studentAttendanceRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentAttendanceRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    var teacherCourse = await _coursRepo.GetRelationSingle(
                        where: x => x.Id == courseId && x.Teacher.Select(x => x.Id).Contains(userId),
                    selector: x => x,
                    returnType: QueryReturnType.FirstOrDefault,
                    asNoTracking: true);
                    if (teacherCourse == null)
                    {
                        return BadRequest("This is not your course");
                    }

                }
                // from course schedule (working days)
                var workingDays = await _courseScheduleRepo.GetRelationList(
                    where: x => x.CourseID == courseId,
                    selector: x => x.DayOfWeek.ToLower(),
                    asNoTracking: true);
                                                            
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
                    var studentsIdInCourse = await _studentRepo.GetRelationList(
                        where: x => x.CourseID == courseId,
                        selector: x => x.Id,
                        asNoTracking: true);



                    var studentsInList = studentAttendance
                        .Any(sa => !studentsIdInCourse.Contains(sa.StudentID));
                       

                    if (studentsInList)
                    {
                        return BadRequest("Some students do not belong to the course.");
                    }
                }

                // date now if exist in database or not  (add if not)
                var today = DateTime.Now.Date;
                if(today < course.StartDate.Date)
                {
                    return BadRequest("The course has not started yet");
                }
                if (today > course.EndDate.Date)
                {
                    return BadRequest("The course is over");
                }
                var existingDate = await _attendanceDateRepo.GetRelationSingle(
                    where: x => x.Date.Date == today,
                    selector: x => x,
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);

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
                var CourseAttExist = await _attendanceDateRepo.GetRelationSingle(
                    where: x => x.Date.Date == today,
                    selector: x => x.CourseAttendance.Any(z => z.CourseId == courseId && z.AttendanceDateId == x.Id),
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);
                if (CourseAttExist)
                {
                    return BadRequest("This course has an attendance today");
                }
                // finaly add student and course attendance
                var newAttendance = new List<StudentAttendance>();
                if (studentAttendance != null)
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "UpdateStudentsAttendance")]
        [HttpPost("newAttendanceForProfile")]
        public async Task<IActionResult> AddAttendanceInProfile(
            [FromForm][Required]int studentId,
            [FromForm][Required] int statusId, 
            [FromForm][Required] DateTime date)
        {
            try
            {
                var Student = await _studentRepo.GetById(studentId);
                if (Student == null)
                {
                    return BadRequest("Student not found");
                }
                var courseAtt = await _attendanceDateRepo.GetRelationAsQueryable(

                                    where: x => x.Date.Date == date.Date,
                                    selector: x => x ==null?null: x.CourseAttendance.Select(z=>z.CourseId));
                if (courseAtt == null)
                {
                    return BadRequest("This date is not a working day");
                }
                else
                {
                    //if (!courseAtt.Contains(Student.CourseID))
                    //{
                    //    return BadRequest("This date is not a working day");
                    //}
                }
                
               
                
                var StudentAttendanceExist = (await _studentAttendanceRepo.GetRelationList
                    (where: x => x.Student.Id == studentId && x.date.Date == date.Date,
                    selector: x => x, asNoTracking: true)).Any();
                if (StudentAttendanceExist)
                {
                    return BadRequest("This student has an attendance on this date");
                }
                bool statusExist = await _attendanceStatusRepo.GetById(statusId) == null;
                if(statusExist)
                {
                    return BadRequest("Status not found");
                }
                var newAtt = new StudentAttendance
                {
                    date = date,
                    StatusID = statusId,
                    StudentID = studentId,
                    CourseID = Student.CourseID
                };
                await _studentAttendanceRepo.Add(newAtt);
                return Ok(new
                {
                    newAtt.Id,
                    newAtt.date,
                    newAtt.StudentID,
                    newAtt.CourseID,
                    newAtt.StatusID
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "UpdateStudentsAttendance")]
        [HttpPatch("Student")]
        public async Task<IActionResult> UpdateStudentAttendance(int id, int statusId)
        {
            
            try
            {
                var studentAttendance = await _studentAttendanceRepo.GetRelationSingle(
                    where: sa => sa.Id == id ,
                    selector: x=>x,
                    returnType: QueryReturnType.SingleOrDefault);
                if (studentAttendance == null)
                {
                    return NotFound("Student Attendance not found.");
                }
                string userId = _studentAttendanceRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentAttendanceRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    var teacherIds = await _coursRepo.GetRelationSingle(
                        where: x => x.Id == studentAttendance.CourseID,
                        include: c => c.Include(z => z.Teacher),
                        selector: x=>x.Teacher.Select(x => x.Id).Contains(userId),
                        returnType: QueryReturnType.SingleOrDefault,
                        asNoTracking: true);
                    if (!teacherIds)
                    {
                        return BadRequest("This attendance is not for your student attendance");
                    }

                }
                studentAttendance.StatusID = statusId;
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
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "AttendanceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "UpdateStudentsAttendance")]
        [HttpDelete("Student")]
        public async Task<IActionResult> DeleteStudentAttendance(int id)
        {
            try
            {
                
                var studentAttendance = await _studentAttendanceRepo.GetRelationSingle(
                    where: sa => sa.Id == id,
                    selector: x => x,
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);
                if (studentAttendance == null)
                {
                    return NotFound("Student Attendance not found.");
                }
                string userId = _studentAttendanceRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentAttendanceRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    var teacherIds = await _coursRepo.GetRelationSingle(
                        where: x => x.Id == studentAttendance.CourseID,
                        include: c => c.Include(z => z.Teacher),
                        selector: x => x.Teacher.Select(x => x.Id).Contains(userId),
                        returnType: QueryReturnType.SingleOrDefault,
                        asNoTracking: true
                        );
                    if (!teacherIds)
                    {
                        return BadRequest("This attendance is not for your student attendance");
                    }

                }

                await _studentAttendanceRepo.Delete(id);
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
