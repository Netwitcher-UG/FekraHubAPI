using AutoMapper;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IRepository<StudentContract> _studentContractRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<AttendanceDate> _attendanceDateRepo;
        private readonly IContractMaker _contractMaker;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public StudentController(IRepository<StudentContract> studentContractRepo, IContractMaker contractMaker,
            IRepository<Student> studentRepo, IRepository<Course> courseRepo, 
            IEmailSender emailSender, IMapper mapper, UserManager<ApplicationUser> userManager, IRepository<AttendanceDate> attendanceDateRepo)
        {
            _studentContractRepo = studentContractRepo;
            _contractMaker = contractMaker;
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
            _emailSender = emailSender;
            _mapper = mapper;
            _userManager = userManager;
            _attendanceDateRepo = attendanceDateRepo;
        }

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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving data from the database : {ex}");
            }
        }

        [Authorize(Policy = "GetStudentsCourse")]

        [HttpGet("GetStudent/{id}")]
        public async Task<IActionResult> GetStudent(int id)////////////////////// Profile for admin
        {
           
            var students = (await _studentRepo.GetRelation()).Where(x => x.Id == id);
            if (!students.Any())
            {
                return NotFound("This student is not found");
            }
            var parent = _userManager.Users.Where(x => x.Id == students.Single().ParentID).SingleOrDefault();
            if (parent == null)
            {
                return NotFound("This student does't have registred parents");
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
                Parent = new
                {
                    parent.Id,
                    parent.FirstName,
                    parent.LastName,
                    parent.Email,
                    parent.PhoneNumber,
                    parent.EmergencyPhoneNumber,
                    parent.Street,
                    parent.StreetNr,
                    parent.ZipCode,
                    parent.City,
                    parent.Nationality,
                    parent.Birthplace,
                    parent.Birthday,
                    parent.Gender,
                    parent.Job,
                    parent.Graduation
                },
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

        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet]
        public async Task<IActionResult> GetStudents(string? search,[Required] int courseId, [FromQuery] PaginationParameters paginationParameters)
        {
            var Allstudents = (await _studentRepo.GetRelation()).Where(x => x.CourseID == courseId);


            if (search != null)
            {
                Allstudents = Allstudents.Where(x => x.FirstName.Contains(search) || x.LastName.Contains(search));
            }
            
            Allstudents = Allstudents.OrderByDescending(x => x.Id);
            var studentsAll = await _studentRepo.GetPagedDataAsync(Allstudents, paginationParameters);
            var att = (await _attendanceDateRepo.GetRelation()).Where(x => x.Date.Date == DateTime.Now.Date).Select(x => x.CourseAttendance.Any(z => z.CourseId == courseId && z.AttendanceDateId == x.Id)).FirstOrDefault();
            var students = studentsAll.Data.Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.Birthday,
                x.Nationality,
                x.Note,
                x.Gender,
                city = x.City ?? "Like parent",
                Street = x.Street ?? "Like parent",
                StreetNr = x.StreetNr ?? "Like parent",
                ZipCode = x.ZipCode ?? "Like parent",
                course = x.Course == null ? null : new
                {
                    x.CourseID,
                    x.Course.Name,
                    x.Course.Capacity,
                    startDate = x.Course.StartDate.Date,
                    EndDate = x.Course.EndDate.Date,
                    x.Course.Price,
                    CourseAttendance = att
                },
                parent = x.User == null ? null : new { x.ParentID, x.User.FirstName, x.User.LastName, x.User.Email, x.User.City, x.User.Street, x.User.StreetNr, x.User.ZipCode }
            }).ToList();
            return Ok(new { studentsAll.TotalCount, studentsAll.PageSize, studentsAll.TotalPages, studentsAll.CurrentPage, students });
        }
        [Authorize(Policy = "ManageChildren")]

        [HttpGet("ByParent")]
        public async Task<IActionResult> GetStudentsByParent()
        {
            var parentId = _courseRepo.GetUserIDFromToken(User);

            if (string.IsNullOrEmpty(parentId))
            {
                return Unauthorized("Parent not found.");
            }

            var students = (await _studentRepo.GetRelation()).Where(x => x.ParentID == parentId).OrderByDescending(x => x.Id);

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
        [Authorize(Policy = "ManageChildren")]

        [HttpPost]
        public async Task<IActionResult> GetContract([FromForm] Map_Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var parentId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(parentId))
                {
                    return Unauthorized("User not found.");
                }

                var studentEntity = new Student
                {
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Nationality = student.Nationality,
                    Note = student.Note,
                    Gender = student.Gender,
                    Birthday = student.Birthday,
                    City = student.City ?? "Like parent",
                    Street = student.Street ?? "Like parent",
                    StreetNr = student.StreetNr ?? "Like parent",
                    ZipCode = student.ZipCode ?? "Like parent",
                    CourseID = student.CourseID,
                    ParentID = parentId,
                };
                var contract = await _contractMaker.ContractHtml(studentEntity);
                return Ok(contract);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating new student record {ex}");
            }
        }
        [Authorize(Policy = "ManageChildren")]

        [HttpGet("GetStudentByParent/{id}")]
        public async Task<IActionResult> GetStudentByParent(int id)
        {
            var parentId = _courseRepo.GetUserIDFromToken(User);

            if (string.IsNullOrEmpty(parentId))
            {
                return Unauthorized("Parent not found.");
            }

            var students = (await _studentRepo.GetRelation()).Where(x => x.ParentID == parentId && x.Id == id);
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
        [Authorize(Policy = "ManageChildren")]

        [HttpPost("AcceptedContract")]
        public async Task<IActionResult> AcceptedContract([FromForm] Map_Student student)
        {
            var userId = _courseRepo.GetUserIDFromToken(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var studentEntity = new Student
                {
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Nationality = student.Nationality,
                    Note = student.Note ?? "",
                    Gender = student.Gender,
                    Birthday = student.Birthday,
                    City = student.City,
                    Street = student.Street,
                    StreetNr = student.StreetNr,
                    ZipCode = student.ZipCode,
                    ParentID = userId,
                    CourseID = student.CourseID,
                };
                await _studentRepo.Add(studentEntity);
                await _contractMaker.ConverterHtmlToPdf(studentEntity);
                var res = await _emailSender.SendContractEmail(studentEntity.Id, $"{studentEntity.FirstName}_{studentEntity.LastName}_Contract");
                if (res is BadRequestObjectResult)
                {
                    await _emailSender.SendContractEmail(studentEntity.Id, $"{studentEntity.FirstName}_{studentEntity.LastName}_Contract");
                }

                return Ok("welcomes your son to our family . A copy of the contract was sent to your email");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [Authorize(Policy = "GetContracts")]
        [HttpGet("Contracts")]
        public async Task<IActionResult> GetContracts()
        {
            try
            {
                var contracts = (await _studentContractRepo.GetRelation()).OrderByDescending(x => x.Id);
                var result = contracts.Select(x => new {
                    x.Id,
                    x.StudentID,
                    x.Student.FirstName,
                    x.Student.LastName,
                    ParentId = x.Student.ParentID,
                    ParentFirstName = x.Student.User.FirstName,
                    ParentLastName = x.Student.User.LastName,
                    x.CreationDate,
                
                }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [Authorize(Policy = "GetContracts")]
        [HttpGet("GetContractsByStudent")]
        public async Task<IActionResult> GetContractsByStudent([Required] int studentId)
        {
            try
            {
                var contracts = (await _studentContractRepo.GetRelation()).Where(x => x.StudentID == studentId).OrderByDescending(x => x.Id);
                var result = contracts.Select(x => new {
                    x.Id,
                    x.StudentID,
                    x.Student.FirstName,
                    x.Student.LastName,
                    ParentId = x.Student.ParentID,
                    ParentFirstName = x.Student.User.FirstName,
                    ParentLastName = x.Student.User.LastName,
                    x.CreationDate,

                }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [Authorize(Policy = "GetContracts")]
        [HttpGet("DownloadContractFileForAdmin")]
        public async Task<ActionResult<IEnumerable<Upload>>> DownloadContractFileForAdmin(int contractId)
        {
            var query = await _studentContractRepo.GetById(contractId);
            if (query == null)
            {
                return BadRequest("file not found");
            }
            var result = Convert.ToBase64String(query.File);

            return Ok(result);
        }

        [Authorize(Policy = "ManageChildren")]

        [HttpGet("SonsContractsForParent")]
        public async Task<IActionResult> GetSonsOfParentContracts()
        {
            try
            {
                var userId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found.");
                }
                var allContracts = await _studentContractRepo.GetRelation();
                var contracts = allContracts.Where(x => x.Student.ParentID == userId).Select(x => new {
                    x.Id,
                    studentId = x.Student.Id,
                    x.Student.FirstName,
                    x.Student.LastName,
                    x.CreationDate,
                  
                }).ToList();
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [Authorize(Policy = "ManageChildren")]

        [HttpGet("SonContractsForParent")]
        public async Task<IActionResult> GetSonOfParentContracts([Required] int studentId)
        {
            try
            {
                var userId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found.");
                }
                var parentId = userId;
                var allContracts = await _studentContractRepo.GetRelation();
                var contracts = allContracts.Where(x => x.Student.ParentID == parentId && x.StudentID == studentId).Select(x => new {
                    x.Id,
                    studentId = x.Student.Id,
                    x.Student.FirstName,
                    x.Student.LastName,
                    x.CreationDate,

                }).ToList();
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [Authorize(Policy = "ManageChildren")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Upload>>> DownloadContractFile(int contractId)
        {
            var query = await _studentContractRepo.GetById(contractId);
            if (query == null)
            {
                return BadRequest("file not found");
            }
            var result = Convert.ToBase64String(query.File);

            return Ok(result);
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
                if(student.ParentID != userId || student == null)
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
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("testing/sendEmails/foreach")]
        public async Task<IActionResult> Testing()
        {
            try
            {
                await _emailSender.SendToAllNewEvent();
                return Ok("Emails sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        //[AllowAnonymous]
        //[HttpGet("testing/contract")]
        //public async Task<IActionResult> TestingContract()
        //{
        //    try
        //    {
        //        var studentEntity = (await _studentRepo.GetRelation()).FirstOrDefault();
        //        byte[] contract = await _contractMaker.ContractHtml(studentEntity);
        //        return File(contract, "application/pdf", "contract.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        //    }
        //}

    }
}
