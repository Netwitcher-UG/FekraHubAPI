using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FekraHubAPI.Controllers.Students
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class StudentController : ControllerBase
    {
        private readonly IRepository<StudentContract> _studentContractRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<CourseSchedule> _courseScheduleRepo;
        private readonly IRepository<AttendanceDate> _attendanceDateRepo;
        private readonly IContractMaker _contractMaker;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentController> _logger;
        public StudentController(IRepository<StudentContract> studentContractRepo, IContractMaker contractMaker,
            IRepository<Student> studentRepo, IRepository<Course> courseRepo,
            IEmailSender emailSender, IMapper mapper,
            UserManager<ApplicationUser> userManager, 
            IRepository<AttendanceDate> attendanceDateRepo,
            IRepository<CourseSchedule> courseScheduleRepo,
            ILogger<StudentController> logger)
        {
            _studentContractRepo = studentContractRepo;
            _contractMaker = contractMaker;
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
            _emailSender = emailSender;
            _mapper = mapper;
            _userManager = userManager;
            _attendanceDateRepo = attendanceDateRepo;
            _courseScheduleRepo = courseScheduleRepo;
            _logger = logger;
        }

        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet("GetStudent/{id}")]
        public async Task<IActionResult> GetStudent(int id)////////////////////// Profile for admin
        {
            try
            {
                var students = await _studentRepo.GetRelation<Student>(x => x.Id == id);
                if (!students.Any())
                {
                    return NotFound("This student is not found");
                }
                var userId = _studentContractRepo.GetUserIDFromToken(User);
                var Teacher = await _studentContractRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    if (!students.Any(x => x.Course.Teacher.Select(z => z.Id).Contains(userId)))
                    {
                        return BadRequest("This student isn't in your course");
                    }
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet]
        public async Task<IActionResult> GetStudents(string? search, int? courseId, [FromQuery] PaginationParameters paginationParameters)
        {
            try
            {
                var Allstudents = (await _studentRepo.GetRelation<Student>(null,
                new List<Expression<Func<Student, bool>>?>
                    {
                        search != null ? (Expression<Func<Student, bool>>)(x => x.FirstName.Contains(search) || x.LastName.Contains(search)) : null,
                        courseId != null ? (Expression<Func<Student, bool>>)(x => x.CourseID == courseId) : null
                    }.Where(x => x != null).Cast<Expression<Func<Student, bool>>>().ToList()
                ));
                string userId = _studentContractRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentContractRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    if (courseId != null)
                    {
                        var course = (await _courseRepo.GetRelation<Course>(x => x.Id == courseId)).FirstOrDefault();
                        if (course != null)
                        {
                            var teacherIds = course.Teacher.Select(x => x.Id);
                            if (!teacherIds.Contains(userId))
                            {
                                return BadRequest("You are not in this course");
                            }
                        }
                        else
                        {
                            return NotFound("Course not found");
                        }

                    }
                    else
                    {
                        var corsesHaveTeacher = await _courseRepo.GetRelation<int>
                            (x => x.Teacher.Select(z => z.Id).Contains(userId), null, z => z.Id);
                        if (corsesHaveTeacher.Any())
                        {
                            Allstudents = Allstudents.Where(x => corsesHaveTeacher.Contains(x.Course.Id));
                        }
                    }
                }
                var studentsAll = await _studentRepo.GetPagedDataAsync(Allstudents.OrderByDescending(x => x.Id), paginationParameters);
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
                        x.Course.Price
                    },
                    parent = x.User == null ? null : new { x.ParentID, x.User.FirstName, x.User.LastName, x.User.Email, x.User.City, x.User.Street, x.User.StreetNr, x.User.ZipCode }
                }).ToList();
                return Ok(new { studentsAll.TotalCount, studentsAll.PageSize, studentsAll.TotalPages, studentsAll.CurrentPage, students });

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet("studentForAttendance")]
        public async Task<IActionResult> GetStudents(string? search, [Required] int courseId)
        {
            try
            {
                string userId = _studentContractRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentContractRepo.IsTeacherIDExists(userId);
                var course = await _courseRepo.GetRelation<Course>(x => x.Id == courseId);
                if (course.FirstOrDefault() != null)
                {
                    if (Teacher)
                    {
                        var teacherIds = course.SelectMany(x => x.Teacher.Select(z => z.Id));
                        if (teacherIds == null)
                        {
                            return BadRequest("This course does not have any teachers");
                        }
                        if (!teacherIds.Contains(userId))
                        {
                            return BadRequest("You are not in this course");
                        }
                    }
                }
                else
                {
                    return NotFound("Course not found");
                }
                var Allstudents = (await _studentRepo.GetRelation<Student>(null,
                    new List<Expression<Func<Student, bool>>?>
                        {
                        x => x.CourseID == courseId,
                        search != null ? (Expression<Func<Student, bool>>)(x => x.FirstName.Contains(search) || x.LastName.Contains(search)) : null
                        }.Where(x => x != null).Cast<Expression<Func<Student, bool>>>().ToList()
                    )).OrderByDescending(x => x.Id);
                var att = (await _attendanceDateRepo.GetRelation<bool>(x => x.Date.Date == DateTime.Now.Date, null,
                    x => x.CourseAttendance.Any(z => z.CourseId == courseId && z.AttendanceDateId == x.Id)))
                    .SingleOrDefault();
                var workingDays = (await _courseScheduleRepo.GetRelation<string>(x => x.CourseID == courseId, null, z => z.DayOfWeek)).ToList();
                bool isTodayIsWorkingDay = workingDays.Contains(DateTime.Now.DayOfWeek.ToString());
                var students = Allstudents.Select(x => new
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
                    studentAttendance = x.StudentAttendance.Where(x => x.date.Date == DateTime.Now.Date)
                                        .Select(x => x.AttendanceStatus.Title)
                                        .SingleOrDefault(),
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
                return Ok(new { IsTodayAWorkDay = isTodayIsWorkingDay, CourseAttendance = att, students });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
    }
}
