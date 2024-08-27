using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                    return Unauthorized("Parent not found.");
                }

                var students = (await _studentRepo.GetRelation<Student>(x => x.ParentID == parentId)).OrderByDescending(x => x.Id);

                var result = students.Select(z => new
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


                }).ToList();


                return Ok(result);
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
                    return Unauthorized("Parent not found.");
                }

                var students = await _studentRepo.GetRelation<Student>(x => x.ParentID == parentId && x.Id == id);
                if (!students.Any())
                {
                    return NotFound("This student is not found");
                }
                var reportNew = students
                    .SelectMany(x => x.Report)
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
                        TeacherLastName = z.User == null ? null : z.User.LastName,
                    });

                var uploadNew = students
                                .SelectMany(x => x.Course.Upload)
                                .Where(upload => upload.Date >= DateTime.Now.AddDays(-30))
                                .Select(upload => new
                                {
                                    upload.Id,
                                    upload.FileName,
                                    upload.Date,
                                    upload.UploadType.TypeTitle
                                })
                                .ToList();
                var invoiceNew = students
                    .SelectMany(x => x.Invoices)
                    .Where(x => x.Date >= DateTime.Now.AddDays(-30))
                    .Select(z => new
                    {
                        z.Id,
                        z.FileName,
                        z.Date,
                    });
                var result = students.Select(z => new
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
                        Report = reportNew == null ? null : reportNew,
                        WorkSheet = uploadNew == null ? null : uploadNew,
                        Invoice = invoiceNew == null ? null : invoiceNew
                    }


                }).FirstOrDefault();


                return Ok(result);
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
                        return Unauthorized("User not found.");
                    }
                    return BadRequest("Invalid student ID");
                };
                student.Nationality = Nationality;
                student.Street = Street;
                student.StreetNr = StreetNr;
                student.ZipCode = ZipCode;
                student.City = City;

                await _studentRepo.Update(student);

                return Ok("Student Data is updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
    }
}
