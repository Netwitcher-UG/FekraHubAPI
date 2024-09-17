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
using Microsoft.EntityFrameworkCore;
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
                var students = await _studentRepo.GetRelationSingle(
                    where:x => x.Id == id,
                    include:x=>x.Include(u=>u.User).Include(z=>z.Course).ThenInclude(z => z.Teacher)
                    .Include(z => z.Course.Room).ThenInclude(z=>z.Location).Include(z=>z.Course.Upload)
                    .Include(z=>z.Report).Include(z => z.Invoices),
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
                        Parent = new
                        {
                            z.User.Id,
                            z.User.FirstName,
                            z.User.LastName,
                            z.User.Email,
                            z.User.PhoneNumber,
                            z.User.EmergencyPhoneNumber,
                            z.User.Street,
                            z.User.StreetNr,
                            z.User.ZipCode,
                            z.User.City,
                            z.User.Nationality,
                            z.User.Birthplace,
                            z.User.Birthday,
                            z.User.Gender,
                            z.User.Job,
                            z.User.Graduation
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
                                TeacherLastName = z.User == null ? null : z.User.LastName,
                            }).ToList(),
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
                            }).ToList()
                        }


                    },
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true);
                if (students == null)
                {
                    return NotFound("This student is not found");
                }
                var userId = _studentContractRepo.GetUserIDFromToken(User);
                var Teacher = await _studentContractRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    if (!students.course.Teacher.Select(x=>x.Id).Contains(userId))
                    {
                        return BadRequest("This student isn't in your course");
                    }
                }
                var parent = await _userManager.Users.AnyAsync(x => x.Id == students.Parent.Id);
                if (!parent)
                {
                    return NotFound("This student does't have registred parents");
                }
               
                return Ok(students);
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
                string userId = _studentContractRepo.GetUserIDFromToken(User);
                bool Teacher = await _studentContractRepo.IsTeacherIDExists(userId);
                List<int>? corsesHaveTeacher = null;
                if (Teacher)
                {
                    if (courseId != null)
                    {
                        var course = await _courseRepo.GetById(courseId ?? 0);
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
                         corsesHaveTeacher = await _courseRepo.GetRelationList(
                            where: x => x.Teacher.Select(z => z.Id).Contains(userId),
                            selector: z => z.Id,
                            asNoTracking: true);
                        
                    }
                }
                var Allstudents = await _studentRepo.GetRelationAsQueryable(
                manyWhere: new List<Expression<Func<Student, bool>>?>
                    {
                        search != null ? (Expression<Func<Student, bool>>)(x => x.FirstName.Contains(search) || x.LastName.Contains(search)) : null,
                        courseId != null ? (Expression<Func<Student, bool>>)(x => x.CourseID == courseId) : null,
                        corsesHaveTeacher != null ? (Expression<Func<Student, bool>>)(x => corsesHaveTeacher.Contains(x.Course.Id)) : null
                    }.Where(x => x != null).Cast<Expression<Func<Student, bool>>>().ToList(),
                include:x=>x.Include(z=>z.User).Include(z=>z.Course),
                orderBy:x=>x.Id,
                selector: x => new
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
                },
                asNoTracking:true
                );

                var studentsAll = await _studentRepo.GetPagedDataAsync(Allstudents, paginationParameters);
                
                return Ok(new { studentsAll.TotalCount, studentsAll.PageSize, studentsAll.TotalPages, studentsAll.CurrentPage, students = studentsAll.Data });

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
                var course = await _courseRepo.GetById(courseId);
                if (course != null)
                {
                    if (Teacher)
                    {
                        var teacherIds = course.Teacher.Select(z => z.Id);
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
                var att = await _attendanceDateRepo.GetRelationSingle(
                    where:x => x.Date.Date == DateTime.Now.Date, 
                    selector: x => x.CourseAttendance.Any(z => z.CourseId == courseId && z.AttendanceDateId == x.Id),
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true
                    ) ;
                var Allstudents = await _studentRepo.GetRelationList(
                    manyWhere: new List<Expression<Func<Student, bool>>?>
                        {
                        x => x.CourseID == courseId,
                        search != null ? (Expression<Func<Student, bool>>)(x => x.FirstName.Contains(search) || x.LastName.Contains(search)) : null
                        }.Where(x => x != null).Cast<Expression<Func<Student, bool>>>().ToList(),
                    orderBy:x=>x.Id,
                    selector: x => new
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
                    }
                    );

                

                var workingDays = await _courseScheduleRepo.GetRelationList(
                    where:x => x.CourseID == courseId,
                    selector: z => z.DayOfWeek,
                    asNoTracking:true);
                bool isTodayIsWorkingDay = workingDays.Contains(DateTime.Now.DayOfWeek.ToString());
                
                return Ok(new { IsTodayAWorkDay = isTodayIsWorkingDay, CourseAttendance = att, Allstudents });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
    }
}
