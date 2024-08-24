﻿using AutoMapper;
using FekraHubAPI.ContractMaker;
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

        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet("GetStudent/{id}")]
        public async Task<IActionResult> GetStudent(int id)////////////////////// Profile for admin
        {

            var students = (await _studentRepo.GetRelation<Student>()).Where(x => x.Id == id);
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
        public async Task<IActionResult> GetStudents(string? search, int? courseId, [FromQuery] PaginationParameters paginationParameters)
        {
            var Allstudents = (await _studentRepo.GetRelation<Student>(null,
                new List<Expression<Func<Student, bool>>?>
                    {
                        search != null ? (Expression<Func<Student, bool>>)(x => x.FirstName.Contains(search) || x.LastName.Contains(search)) : null,
                        courseId != null ? (Expression<Func<Student, bool>>)(x => x.CourseID == courseId) : null
                    }.Where(x => x != null).Cast<Expression<Func<Student, bool>>>().ToList()
                )).OrderByDescending(x => x.Id);

            var studentsAll = await _studentRepo.GetPagedDataAsync(Allstudents, paginationParameters);
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
        [Authorize(Policy = "GetStudentsCourse")]
        [HttpGet("studentForAttendance")]
        public async Task<IActionResult> GetStudents(string? search, [Required] int courseId)
        {
            var Allstudents = (await _studentRepo.GetRelation<Student>(null,
                new List<Expression<Func<Student, bool>>?>
                    {
                        x => x.CourseID == courseId,
                        search != null ? (Expression<Func<Student, bool>>)(x => x.FirstName.Contains(search) || x.LastName.Contains(search)) : null
                    }.Where(x => x != null).Cast<Expression<Func<Student, bool>>>().ToList()



                )).OrderByDescending(x => x.Id);

            var att = (await _attendanceDateRepo.GetRelation<bool>(x => x.Date.Date == DateTime.Now.Date, null,
                x => x.CourseAttendance.Any(z => z.CourseId == courseId && z.AttendanceDateId == x.Id)))
                .FirstOrDefault();
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
            return Ok(new { CourseAttendance = att, students });
        }
    }
}
