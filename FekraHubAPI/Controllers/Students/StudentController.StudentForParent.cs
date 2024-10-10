using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Controllers.Students
{
    public partial class StudentController
    {
        [Authorize(Policy = "ManageChildren")]
        [HttpGet("CoursesCapacity")]
        public async Task<IActionResult> GetCoursesWithCapacity()
        {
            try
            {
                var courses = await _courseRepo.GetAll();
                var allStudentsInCourses = await _studentRepo.GetAll();

                foreach (var course in courses)
                {
                    course.Capacity -= allStudentsInCourses.Count(c => c.CourseID == course.Id);
                }
                var courseInfo = courses.Where(x => x.Capacity > 0).Select(x => new { x.Id, x.Name, x.Capacity, x.StartDate, x.EndDate, x.Lessons, x.Price }).ToList();
                return Ok(courseInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }


        [Authorize(Policy = "ManageChildren")]
        [HttpGet("ByParent")]
        public async Task<IActionResult> GetStudentsByParent()
        {
            try
            {
                var parentId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(parentId))
                {
                    return Unauthorized("Elternteil nicht gefunden.");//Parent not found.
                }

                var students = await _studentRepo.GetRelationList(
                    where:x => x.ParentID == parentId && x.ActiveStudent == true,
                    orderBy: x => x.Id,
                    include:x=> x.Include(t => t.Course.Teacher).Include(c=>c.Course).ThenInclude(r=>r.Room).ThenInclude(l=>l.Location),
                    selector: z => new
                    {
                        z.Id,
                        z.FirstName,
                        z.LastName,
                        z.Birthday,
                        z.Nationality,
                        z.Note,
                        z.Gender,
                        city = z.City ?? "Like parent",
                        Street = z.Street ?? "Like parent",
                        StreetNr = z.StreetNr ?? "Like parent",
                        ZipCode = z.ZipCode ?? "Like parent",
                        course = z.Course == null ? null : new
                        {
                            z.Course.Id,
                            z.Course.Name,
                            z.Course.Capacity,
                            startDate = z.Course.StartDate.Date,
                            EndDate = z.Course.EndDate.Date,
                            z.Course.Price,
                            Teacher = z.Course.Teacher.Select(x => new
                            {
                                x.Id,
                                x.FirstName,
                                x.LastName

                            })
                        },
                        Room = z.Course == null ? null : new
                        {
                            z.Course.Room.Id,
                            z.Course.Room.Name
                        },
                        Location = z.Course == null ? null : new
                        {
                            z.Course.Room.Location.Id,
                            z.Course.Room.Location.Name,
                            z.Course.Room.Location.City,
                            z.Course.Room.Location.Street,
                            z.Course.Room.Location.ZipCode,
                            z.Course.Room.Location.StreetNr
                        }


                    },
                    asNoTracking:true
                    );

                


                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "ManageChildren")]
        [HttpGet("GetStudentByParent/{id}")]
        public async Task<IActionResult> GetStudentByParent(int id)
        {
            try
            {
                var parentId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(parentId))
                {
                    return Unauthorized("Elternteil nicht gefunden.");//Parent not found.
                }

                var students = await _studentRepo.GetRelationSingle(
                    where:x => x.ParentID == parentId && x.Id == id && x.ActiveStudent == true,
                    returnType:QueryReturnType.SingleOrDefault,
                    include:x=>x.Include(r=>r.Report).Include(c=>c.Course).ThenInclude(u=>u.Upload).Include(i=>i.Invoices)
                    .Include(z=>z.Course.Room).ThenInclude(z=>z.Location).Include(z => z.Course.Teacher),
                    selector: z => new
                    {
                        z.Id,
                        z.FirstName,
                        z.LastName,
                        z.Birthday,
                        z.Nationality,
                        z.Note,
                        z.Gender,
                        city = z.City ?? "Like parent",
                        Street = z.Street ?? "Like parent",
                        StreetNr = z.StreetNr ?? "Like parent",
                        ZipCode = z.ZipCode ?? "Like parent",
                        course = z.Course == null ? null : new
                        {
                            z.Course.Id,
                            z.Course.Name,
                            z.Course.Capacity,
                            startDate = z.Course.StartDate.Date,
                            EndDate = z.Course.EndDate.Date,
                            z.Course.Price,
                            Teacher = z.Course.Teacher.Select(x => new
                            {
                                x.Id,
                                x.FirstName,
                                x.LastName

                            })
                        },
                        Room = z.Course == null ? null : new
                        {
                            z.Course.Room.Id,
                            z.Course.Room.Name
                        },
                        Location = z.Course == null ? null : new
                        {
                            z.Course.Room.Location.Id,
                            z.Course.Room.Location.Name,
                            z.Course.Room.Location.City,
                            z.Course.Room.Location.Street,
                            z.Course.Room.Location.ZipCode,
                            z.Course.Room.Location.StreetNr
                        },
                        News = new
                        {
                            Report = z.Report == null ? null : z.Report
                                            .Where(x => x.CreationDate >= DateTime.Now.AddDays(-30))
                                            .Select(z => new
                                            {
                                                z.Id,
                                                z.data,
                                                z.CreationDate,
                                                z.CreationDate.Year,
                                                z.CreationDate.Month,
                                                TeacherId = z.UserId,
                                                TeacherFirstName = z.User == null ? null : z.User.FirstName,
                                                TeacherLastName = z.User == null ? null : z.User.LastName
                                            })
                                            .ToList(),

                            WorkSheet = z.Course == null || z.Course.Upload == null ? null : z.Course.Upload
                                             .Where(upload => upload.Date >= DateTime.Now.AddDays(-30))
                                             .Select(upload => new
                                             {
                                                 upload.Id,
                                                 upload.FileName,
                                                 upload.Date,
                                                 upload.UploadType.TypeTitle
                                             })
                                             .ToList(),

                            Invoice = z.Invoices == null ? null : z.Invoices
                                             .Where(x => x.Date >= DateTime.Now.AddDays(-30))
                                             .Select(z => new
                                             {
                                                 z.Id,
                                                 z.FileName,
                                                 z.Date,
                                             })
                                             .ToList()
                        }
                    },
                    asNoTracking:true);
                if (students == null)
                {
                    return BadRequest("Dieser Schüler wurde nicht gefunden");//This student is not found
                }
               


                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "ManageChildren")]
        [HttpPost("UpdateSonDataFromProfile")]
        public async Task<IActionResult> UpdateSonDataFromProfile(
            [FromForm] int studentId,
            [FromForm] string Nationality,
            [FromForm] string? Street,
            [FromForm] string? StreetNr,
            [FromForm] string? ZipCode,
            [FromForm] string? City
            )
        {
            try
            {
                var userId = _courseRepo.GetUserIDFromToken(User);
                var student = await _studentRepo.GetById(studentId);
                if (student.ParentID != userId || student == null)
                {
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized("Benutzer nicht gefunden.");//User not found.
                    }
                    return BadRequest("Ungültige Schüler-ID.");//Invalid student ID
                };
                student.Nationality = Nationality;
                student.Street = Street;
                student.StreetNr = StreetNr;
                student.ZipCode = ZipCode;
                student.City = City;

                await _studentRepo.Update(student);

                return Ok("Schülerdaten wurden aktualisiert.");//Student Data is updated
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
    }
}
