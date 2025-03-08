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
        private readonly IRepository<Event> _eventRepo;
        private readonly IRepository<CourseSchedule> _courseScheduleRepo;
        private readonly IRepository<AttendanceDate> _attendanceDateRepo;
        private readonly IRepository<Room> _roomRepo;
        private readonly IRepository<Report> _reportRepo;
        private readonly IRepository<Invoice> _invoiceRepo;
        private readonly IRepository<Upload> _uploadRepo;
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
            ILogger<StudentController> logger, IRepository<Event> eventRepo,
            IRepository<Room> roomRepo, IRepository<Report> reportRepo,
            IRepository<Invoice> invoiceRepo, IRepository<Upload> uploadRepo)
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
            _eventRepo = eventRepo;
            _roomRepo = roomRepo;
            _reportRepo = reportRepo;
            _invoiceRepo = invoiceRepo;
            _uploadRepo = uploadRepo;
        }

        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet("GetStudent/{id}")]
        public async Task<IActionResult> GetStudent(int id)////////////////////// Profile for admin
        {
            try
            {
                var student = await _studentRepo.GetRelationSingle(
                            where: x => x.Id == id,
                            selector: z => new
                            {
                                z.Id,
                                z.FirstName,
                                z.LastName,
                                z.Birthday,
                                z.Nationality,
                                z.Note,
                                z.Gender,
                                z.ActiveStudent,
                                City = z.City ?? "Like parent",
                                Street = z.Street ?? "Like parent",
                                StreetNr = z.StreetNr ?? "Like parent",
                                ZipCode = z.ZipCode ?? "Like parent",
                                z.ParentID
                            },
                            returnType: QueryReturnType.SingleOrDefault,
                            asNoTracking: true
                        );

                var parent = await _userManager.Users
                            .Where(x => x.Id == student.ParentID)
                            .Select(x => new
                            {
                                x.Id,
                                x.FirstName,
                                x.LastName,
                                x.Email,
                                x.PhoneNumber,
                                x.EmergencyPhoneNumber,
                                x.Street,
                                x.StreetNr,
                                x.ZipCode,
                                x.City,
                                x.Nationality,
                                x.Birthplace,
                                x.Birthday,
                                x.Gender,
                                x.Job,
                                x.Graduation
                            })
                            .AsNoTracking()
                            .SingleOrDefaultAsync();
                var course = await _courseRepo.GetRelationSingle(
                                where: c => c.Student.Any(s => s.Id == id),
                                include: c => c.Include(x => x.Teacher),
                                selector: c => new
                                {
                                    c.Id,
                                    c.Name,
                                    c.Capacity,
                                    StartDate = c.StartDate.Date,
                                    EndDate = c.EndDate.Date,
                                    c.Price,
                                    Teacher = c.Teacher.Select(t => new
                                    {
                                        t.Id,
                                        t.FirstName,
                                        t.LastName
                                    })
                                },
                                returnType: QueryReturnType.SingleOrDefault,
                                asNoTracking: true
                            );
                var room = course == null ? null :await _courseRepo.GetRelationSingle(
                                where: r => r.Id == course.Id,
                                include: r => r.Include(x => x.Room).ThenInclude(x=>x.Location),
                                selector: r => new
                                {
                                    room = new
                                    {
                                        r.Id,
                                        r.Name
                                    },
                                    Location = r.Room.Location == null ? null : new
                                    {
                                        r.Room.Location.Id,
                                        r.Room.Location.Name,
                                        r.Room.Location.City,
                                        r.Room.Location.Street,
                                        r.Room.Location.ZipCode,
                                        r.Room.Location.StreetNr
                                    }
                                },
                                returnType: QueryReturnType.SingleOrDefault,
                                asNoTracking: true
                            );
                var reports = await _reportRepo.GetRelationList(
                                    where: r => r.StudentId == id && r.CreationDate >= DateTime.Now.AddDays(-30),
                                    selector: r => new
                                    {
                                        r.Id,
                                        r.data,
                                        r.CreationDate,
                                        TeacherId = r.UserId,
                                        TeacherFirstName = r.User.FirstName,
                                        TeacherLastName = r.User.LastName
                                    },asNoTracking:true
                                    );
                var uploads = course == null ? null : await _uploadRepo.GetRelationList(
                                    where: u => u.Courses.Any(c => c.Id == course.Id) && u.Date >= DateTime.Now.AddDays(-30),
                                    selector: u => new
                                    {
                                        u.Id,
                                        u.FileName,
                                        u.Date,
                                        UploadType = u.UploadType.TypeTitle
                                    },
                                    asNoTracking: true
                                );
                var invoices = await _invoiceRepo.GetRelationList(
                                    where: i => i.Studentid == id && i.Date >= DateTime.Now.AddDays(-30),
                                    selector: i => new
                                    {
                                        i.Id,
                                        i.FileName,
                                        i.Date
                                    },
                                    asNoTracking: true
                                );


               var students =  new
                    {
                        student.Id,
                        student.FirstName,
                        student.LastName,
                        student.Birthday,
                        student.Nationality,
                        student.Note,
                        student.Gender,
                        student.ActiveStudent,
                        city = student.City ?? "Like parent",
                        Street = student.Street ?? "Like parent",
                        StreetNr = student.StreetNr ?? "Like parent",
                        ZipCode = student.ZipCode ?? "Like parent",
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
                        course = course == null ? null : new
                        {
                            course.Id,
                            course.Name,
                            course.Capacity,
                            startDate = course.StartDate.Date,
                            EndDate = course.EndDate.Date,
                            course.Price,
                            Teacher = course.Teacher
                        },
                        Room = room == null ? null : room.room,
                        Location = room == null ? null : room.Location,
                        News = new
                        {
                            Report = reports == null ? null : reports,
                            WorkSheet = uploads == null ? null : uploads
                                ,
                            Invoice = invoices == null ? null : invoices
                        }


                    };
                if (students == null)
                {
                    return BadRequest("Dieser Schüler wurde nicht gefunden.");//This student is not found
                }
                var userId = _studentContractRepo.GetUserIDFromToken(User);
                var Teacher = await _studentContractRepo.IsTeacherIDExists(userId);
                if (Teacher)
                {
                    if (!students.course.Teacher.Select(x => x.Id).Contains(userId))
                    {
                        return BadRequest("Dieser Schüler ist nicht in Ihrem Kurs.");//This student isn't in your course
                    }
                }
                var parents = await _userManager.Users.AnyAsync(x => x.Id == students.Parent.Id);
                if (!parents)
                {
                    return BadRequest("Dieser Schüler hat keine registrierten Eltern.");//This student does't have registred parents
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
                                return BadRequest("Sie sind nicht in diesem Kurs.");//You are not in this course
                            }
                        }
                        else
                        {
                            return BadRequest("Kurs nicht gefunden.");//Course not found
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
                include: x => x.Include(z => z.User).Include(z => z.Course),
                orderBy: x => x.Id,
                selector: x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.Birthday,
                    x.Nationality,
                    x.Note,
                    x.Gender,
                    x.ActiveStudent,
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
                asNoTracking: true
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
                var course = await _courseRepo.GetRelationSingle(
                    where: x => x.Id == courseId,
                    include: x => x.Include(x => x.Teacher),
                    selector: x => x,
                    returnType: QueryReturnType.SingleOrDefault
                    );
                if (course == null)
                {
                    return BadRequest("Kurs nicht gefunden.");//Course not found
                }
                var today = DateTime.Now.Date;
                var courseScheduleIds = await _courseScheduleRepo.GetRelationList(
                    where: x => x.CourseID == courseId, selector: x => x.Id);
                var eventIsExist = await _eventRepo.DataExist(x => today >= x.StartDate.Date && today <= x.EndDate.Date &&
                x.CourseSchedule.Any(cs => courseScheduleIds.Contains(cs.Id)));
                if (eventIsExist)
                {
                    return Ok(new { IsTodayAWorkDay = false, CourseAttendance = false, students = new List<Student>() { } });
                }
                if (Teacher)
                {
                    var teacherIds = course.Teacher.Select(z => z.Id);
                    if (teacherIds == null)
                    {
                        return BadRequest("Dieser Kurs hat keine Lehrer.");//This course does not have any teachers
                    }
                    if (!teacherIds.Contains(userId))
                    {
                        return BadRequest("Sie sind nicht in diesem Kurs.");//You are not in this course
                    }
                }


                if (DateTime.Now.Date < course.StartDate.Date)
                {
                    return BadRequest("Der Kurs hat noch nicht begonnen.");//The course has not started yet
                }
                else if (DateTime.Now.Date > course.EndDate.Date)
                {
                    return BadRequest("Der Kurs ist vorbei.");//The course is over
                }
                var att = await _attendanceDateRepo.GetRelationSingle(
                    where: x => x.Date.Date == DateTime.Now.Date,
                    selector: x => x.CourseAttendance.Any(z => z.CourseId == courseId && z.AttendanceDateId == x.Id),
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true
                    );
                var workingDays = await _courseScheduleRepo.GetRelationList(
                    where: x => x.CourseID == courseId,
                    selector: z => z.DayOfWeek,
                    asNoTracking: true);
                bool isTodayIsWorkingDay = workingDays.Contains(DateTime.Now.DayOfWeek.ToString());
                if (!isTodayIsWorkingDay)
                {
                    return Ok(new { IsTodayAWorkDay = isTodayIsWorkingDay, CourseAttendance = att, students = new List<Student>() { } });
                }
                var students = await _studentRepo.GetRelationList(
                    manyWhere: new List<Expression<Func<Student, bool>>?>
                        {
                        
                        x => x.CourseID == courseId,
                        x => x.ActiveStudent == true,
                        search != null ? (Expression<Func<Student, bool>>)(x => x.FirstName.Contains(search) || x.LastName.Contains(search)) : null
                        }.Where(x => x != null).Cast<Expression<Func<Student, bool>>>().ToList(),
                    orderBy: x => x.Id,
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





                return Ok(new { IsTodayAWorkDay = isTodayIsWorkingDay, CourseAttendance = att, students });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [Authorize(Policy = "GetStudentsCourse")]
        [HttpPatch("ActiveStudent")]
        public async Task<IActionResult> ActiveStudent([Required]int id,[Required]bool active)
        {
            var student = await _studentRepo.GetById(id);
            student.ActiveStudent = active;
            await _studentRepo.Update(student);
            return Ok("Erfolg");//success
        }
    }
}
