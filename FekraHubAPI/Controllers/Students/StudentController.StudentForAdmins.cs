using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.Helpers;
using FekraHubAPI.MapModels;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Net;
using System.Text;

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
        private readonly IRepository<Notifications> _notificationsRepo;
        private readonly IRepository<NotificationUser> _notificationUserRepo;

        public StudentController(IRepository<StudentContract> studentContractRepo, IContractMaker contractMaker,
            IRepository<Student> studentRepo, IRepository<Course> courseRepo,
            IEmailSender emailSender, IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IRepository<AttendanceDate> attendanceDateRepo,
            IRepository<CourseSchedule> courseScheduleRepo,
            ILogger<StudentController> logger, IRepository<Event> eventRepo,
            IRepository<Room> roomRepo, IRepository<Report> reportRepo,
            IRepository<Invoice> invoiceRepo, IRepository<Upload> uploadRepo, IRepository<Notifications> notificationsRepo,
            IRepository<NotificationUser> notificationUserRepo)
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
            _notificationsRepo = notificationsRepo;
            _notificationUserRepo = notificationUserRepo;
        }
        
        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet("GetStudent/{id}")]
        public async Task<IActionResult> GetStudent(int id)////////////////////// Profile for admin
        {
            try
            {
                var student = await _studentRepo.GetRelationSingle(
                            where: x => x.Id == id && x.ActiveStudent,
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
                if( student == null )
                {
                    return BadRequest();//////////////////////
                }

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
                if(parent == null)
                {
                    return BadRequest();///////////////////////////
                }
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
                                    where: r => r.StudentId == id && r.CreationDate >= DateTime.UtcNow.AddDays(-30),
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
                                    where: u => u.Courses.Any(c => c.Id == course.Id) && u.Date >= DateTime.UtcNow.AddDays(-30),
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
                                    where: i => i.Studentid == id && i.Date >= DateTime.UtcNow.AddDays(-30),
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
                        (Expression<Func<Student, bool>>)(x => x.ActiveStudent == true),
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
                var today = DateTime.UtcNow.Date;
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


                if (DateTime.UtcNow.Date < course.StartDate.Date)
                {
                    return BadRequest("Der Kurs hat noch nicht begonnen.");//The course has not started yet
                }
                else if (DateTime.UtcNow.Date > course.EndDate.Date)
                {
                    return BadRequest("Der Kurs ist vorbei.");//The course is over
                }
                var att = await _attendanceDateRepo.GetRelationSingle(
                    where: x => x.Date.Date == DateTime.UtcNow.Date.ToUtcSafe(),
                    selector: x => x.CourseAttendance.Any(z => z.CourseId == courseId && z.AttendanceDateId == x.Id),
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true
                    );
                var workingDays = await _courseScheduleRepo.GetRelationList(
                    where: x => x.CourseID == courseId,
                    selector: z => z.DayOfWeek,
                    asNoTracking: true);
                bool isTodayIsWorkingDay = workingDays.Contains(DateTime.UtcNow.DayOfWeek.ToString());
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
                        studentAttendance = x.StudentAttendance.Where(x => x.date.Date == DateTime.UtcNow.Date.ToUtcSafe())
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

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateCourseStudent")]
        public async Task<IActionResult> UpdateCourseStudent(
        [FromForm] int studentId,
         [FromForm] int? CourseId
)
        {

            try
            {
                var student = await _studentRepo.GetById(studentId);
                if(student == null)
                {
                    return BadRequest("Dieser Schüler wurde nicht gefunden.");
                }
                if (CourseId != null && CourseId != 0)
                {
                    var courseExist = await _courseRepo.DataExist(x => x.Id == CourseId.Value);
                    if (!courseExist)
                    {
                        return BadRequest("Kurs nicht gefunden.");
                    }
                    if (student.CourseID != CourseId)
                    {
                        student.CourseID = CourseId;
                        await _studentRepo.Update(student);
                    }
                }
                else
                {
                    student.CourseID = null;
                    await _studentRepo.Update(student);
                }

                return Ok("Schülerdaten wurden aktualisiert.");//Student Data is updated
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }


        [Authorize(Policy = "StudentAdmissions")]
        [HttpGet("pending-student")]
        public async Task<IActionResult> PendingStudents()
        {
            var students = await _studentRepo.GetRelationList(
                where: x => x.ActiveStudent == false,
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
                    Parent = new
                    {
                        x.User.Id,
                        x.User.FirstName,
                        x.User.LastName,
                        x.User.Email,
                        x.User.PhoneNumber,
                        x.User.EmergencyPhoneNumber,
                        x.User.Street,
                        x.User.StreetNr,
                        x.User.ZipCode,
                        x.User.City,
                        x.User.Nationality,
                        x.User.Birthplace,
                        x.User.Birthday,
                        x.User.Gender,
                        x.User.Job,
                        x.User.Graduation,
                    },
                    CanAccept = x.User.EmailConfirmed
                },
                asNoTracking: true
                );
            return Ok(students);
        }
        public class AcceptStudent
        {
            public int StudentId { get; set; }
            public decimal RegistrationFee { get; set; }
            public decimal AnnualCourseFee { get; set; }
            public int? CourseId { get; set; }


        }
        [Authorize(Policy = "StudentAdmissions")] 
        [HttpPost("accept-student")]
        public async Task<IActionResult> AcceptPendingStudent([FromBody] AcceptStudent data)
        {
            var student = await _studentRepo.GetById(data.StudentId);
            if (student == null)
            {
                return BadRequest("Dieser Schüler wurde nicht gefunden.");
            }
            var parent = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == student.ParentID);
            if (parent == null)
            {
                return BadRequest("Das Elternkonto der Erziehungsberechtigten wurde nicht gefunden.");// رسالة بالالماني حساب الاهل غير موجود 
            }
            if (!parent.EmailConfirmed)
            {
                return BadRequest("Die E-Mail-Adresse des Elternkontos ist noch nicht bestätigt");// رسالة بالالماني ايميل الاهل غير مؤكد
            }
            
            if (data.CourseId != null && data.CourseId != 0)
            {
                var courseExist = await _courseRepo.DataExist(x => x.Id == data.CourseId.Value);
                if (!courseExist)
                {
                    return BadRequest("Kurs nicht gefunden.");
                }
                student.CourseID = data.CourseId;
            }
            else
            {
                student.CourseID = null;
            }
            var pdf = await _contractMaker.ConverterHtmlToPdf(student, data.RegistrationFee, data.AnnualCourseFee);
            if(pdf == null)
            {
                return BadRequest("pdf not found");
            }
            student.AdminApproved = true;
            await _studentRepo.Update(student);


            
            await _emailSender.AcceptStudent(parent, student,pdf);

            var newNotification = new Notifications
            {
                Notification = $"{student.FirstName} {student.LastName} wurde aufgenommen. |/children/",
            };
            await _notificationsRepo.Add(newNotification);
            List<NotificationUser> notificationUsers = new List<NotificationUser>();
            var notificationUser = new NotificationUser
            {
                NotificationId = newNotification.Id,
                UserId = parent.Id
            };
            notificationUsers.Add(notificationUser);

            await _notificationUserRepo.ManyAdd(notificationUsers);
            return Ok();
        }
        public class RejectData
        {
            public int StudentId { get; set; }
            public string? Reason { get; set; }
        }
        [Authorize(Policy = "StudentAdmissions")]
        [HttpPost("reject-student")]
        public async Task<IActionResult> RejectStudent([FromBody] RejectData rejectData)
        {
            var student = await _studentRepo.GetById(rejectData.StudentId);
            if (student == null)
            {
                return BadRequest("Dieser Schüler wurde nicht gefunden.");
            }
            var parent = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == student.ParentID);
            if (parent == null)
            {
                return BadRequest("Das Elternkonto der Erziehungsberechtigten wurde nicht gefunden.");// رسالة بالالماني حساب الاهل غير موجود 
            }
            await _studentRepo.Delete(student);
            await _emailSender.RejectStudentForParent(parent, rejectData.Reason ?? "");

            var newNotification = new Notifications
            {
                Notification = $"{student.FirstName} {student.LastName} wurde abgelehnt.|/children/",
            };
            await _notificationsRepo.Add(newNotification);
            var notificationUser = new NotificationUser
            {
                NotificationId = newNotification.Id,
                UserId = parent.Id
            };
            await _notificationUserRepo.Add(notificationUser);
            return Ok();
        }
        public class AcceptContract
        {
            public string Token { get; set; }
            public string ParentId { get; set; }
            public int StudentId { get; set; }
        }

        [HttpPost("accept-contract")]
        public async Task<IActionResult> AcceptStudentFromParent([FromBody] AcceptContract data)
        {
            if (string.IsNullOrWhiteSpace(data.Token) ||
                string.IsNullOrWhiteSpace(data.ParentId) ||
                data.StudentId <= 0)
            {
                return BadRequest("Fehlende oder ungültige Parameter."); // missing_or_invalid_parameters
            }
            var student = await _studentRepo.GetRelationSingle(
                where: x => x.Id == data.StudentId,
                selector: x => x
                );
            
            if (student == null)
            {
                return BadRequest("Schüler nicht gefunden."); // student_not_found
            }
            if (student.AdminApproved != true)
            {
                return BadRequest("Der Administrator hat diesen Schüler noch nicht genehmigt."); // admin_not_approved
            }

            var parent = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == data.ParentId);
            if (parent == null)
            {
                return BadRequest("Elternkonto nicht gefunden."); // parent_not_found
            }
            if (student.ParentID != data.ParentId)
                return BadRequest("Schüler stimmt nicht mit dem Elternkonto überein."); // student_parent_mismatch


            string rawToken;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(data.Token);
                rawToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                return BadRequest("Ungültiges Token-Format."); // invalid_token_format
            }

            var provider = _userManager.Options.Tokens.EmailConfirmationTokenProvider;
            var isValid = await _userManager.VerifyUserTokenAsync(
                parent,
                provider,
                "EmailConfirmation",
                rawToken
            );

            if (!isValid)
            {
                return BadRequest("Ungültiges oder abgelaufenes Token."); // invalid_or_expired_token
            }
            if(student.ParentApproved == true)
            {
                return BadRequest("Bereits genehmigt."); // already_approved
            }
            student.ParentApproved = true;
            if(student.AdminApproved == true)
            {
                student.ActiveStudent = true;
            }
            await _studentRepo.Update(student);

            var newNotification = new Notifications
            {
                Notification = $"Vertrag bestätigt({student.FirstName} {student.LastName}).|/students/",//children
            };
            await _notificationsRepo.Add(newNotification);
            var notificationUser = new NotificationUser
            {
                NotificationId = newNotification.Id,
                UserId = parent.Id
            };
            await _notificationUserRepo.Add(notificationUser);
            return Ok("Erfolgreich genehmigt.");
        }


        public class Map_Student_Update
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Gender { get; set; }
            public DateTime? Birthday { get; set; }
            public string? Nationality { get; set; }
            public string? Note { get; set; }
            public string? Street { get; set; }
            public string? StreetNr { get; set; }
            public string? ZipCode { get; set; }
            public string? City { get; set; }
            

        }
        [Authorize(Policy = "ManageStudents")]
        [HttpPut("student-info/{Id}")]   
        public async Task<IActionResult> UpdateStudentInfo(int Id ,[FromBody] Map_Student_Update studentInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var student = await _studentRepo.GetById(Id);
            if (student == null)
            {
                return BadRequest("Dieser Schüler wurde nicht gefunden.");
            }
            if(!string.IsNullOrEmpty(studentInfo.FirstName))
            {
                student.FirstName = studentInfo.FirstName ;
            }
            if (!string.IsNullOrEmpty(studentInfo.LastName))
            {
                student.LastName = studentInfo.LastName;
            }
            if (!string.IsNullOrEmpty(studentInfo.Gender))
            {
                student.Gender = studentInfo.Gender;
            }
            if (!string.IsNullOrEmpty(studentInfo.Nationality))
            {
                student.Nationality = studentInfo.Nationality;
            }
            if (!string.IsNullOrEmpty(studentInfo.Street))
            {
                student.Street = studentInfo.Street;
            }
            if (!string.IsNullOrEmpty(studentInfo.StreetNr))
            {
                student.StreetNr = studentInfo.StreetNr;
            }
            if (!string.IsNullOrEmpty(studentInfo.City))
            {
                student.City = studentInfo.City;
            }
            if (!string.IsNullOrEmpty(studentInfo.ZipCode))
            {
                student.ZipCode = studentInfo.ZipCode;
            }
            var minBirthDate = new DateTime(1980, 1, 1);

            if (studentInfo.Birthday.HasValue && studentInfo.Birthday.Value > minBirthDate)
            {
                student.Birthday = studentInfo.Birthday.Value.ToUtcSafe();
            }

            await _studentRepo.Update(student);

            return Ok(new
            {
                student.FirstName,student.LastName,student.Gender,student.Birthday,student.Nationality,student.Street,student.StreetNr,student.City,student.ZipCode
            });    
        }

        [HttpDelete("student-info/{Id}")]
        public async Task<IActionResult> DeleteStudentInfo(int Id)
        {
            
            var student = await _studentRepo.GetById(Id);
            if (student == null)
            {
                return BadRequest("Dieser Schüler wurde nicht gefunden.");
            }

            student.ActiveStudent = false;
            await _studentRepo.Update(student);

            return Ok();
        }

    }
}
