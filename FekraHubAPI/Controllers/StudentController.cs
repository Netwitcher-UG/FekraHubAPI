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
using System.Security.Claims;

namespace FekraHubAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IRepository<StudentContract> _studentContractRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IContractMaker _contractMaker;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public StudentController(IRepository<StudentContract> studentContractRepo, IContractMaker contractMaker,
            IRepository<Student> studentRepo, IRepository<Course> courseRepo, IEmailSender emailSender, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _studentContractRepo = studentContractRepo;
            _contractMaker = contractMaker;
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
            _emailSender = emailSender;
            _mapper = mapper;
            _userManager = userManager;
        }

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
                var courseInfo = courses.Where(x => x.Capacity > 0).Select(x => new { x.Id, x.Name, x.Capacity,x.StartDate,x.EndDate,x.Lessons,x.Price }).ToList();
                return Ok(courseInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetStudents(string? search , int? courseId, [FromQuery] PaginationParameters paginationParameters)
        {
            var Allstudents = await _studentRepo.GetRelation();


            if (search != null)
            {
                Allstudents = Allstudents.Where(x => x.FirstName.Contains(search) || x.LastName.Contains(search));
            }
            if(courseId != null)
            {
                Allstudents = Allstudents.Where(x => x.CourseID == courseId);
            }
            Allstudents = Allstudents.OrderByDescending(x => x.Id);
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
            return Ok(new {  studentsAll.TotalCount,studentsAll.PageSize, studentsAll.TotalPages, studentsAll.CurrentPage, students });
        }
        [HttpGet("ByParent")]
        public async Task<IActionResult> GetStudentsByParent()
        {
            var parentId =_courseRepo.GetUserIDFromToken(User);

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
                { z.Course.Id, z.Course.Name, z.Course.Capacity,startDate = z.Course.StartDate.Date,
                    EndDate = z.Course.EndDate.Date, z.Course.Price }
            }).ToList();


            return Ok(result);
        }
        [AllowAnonymous]
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
                    CourseID = student.CourseID ,
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
                    Note = student.Note,
                    Gender = student.Gender,
                    Birthday = student.Birthday,
                    City = student.City,
                    Street = student.Street,
                    StreetNr = student.StreetNr,
                    ZipCode = student.ZipCode ,
                    ParentID = userId,
                    CourseID = student.CourseID,
                };
                await _studentRepo.Add(studentEntity);
                await _contractMaker.ConverterHtmlToPdf(studentEntity);
                var res = await _emailSender.SendContractEmail(studentEntity.Id, "Son_Contract");
                if (res is BadRequestObjectResult)
                {
                    await _emailSender.SendContractEmail(studentEntity.Id, "Son_Contract");
                }

                return Ok("welcomes your son to our family . A copy of the contract was sent to your email");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

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
                    x.File
                }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
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
                var parentId = userId;
                var allContracts = await _studentContractRepo.GetRelation();
                var contracts = allContracts.Where(x => x.Student.ParentID == parentId).Select(x => new {
                    x.Id,
                    studentId = x.Student.Id,
                    x.Student.FirstName,
                    x.Student.LastName,
                    x.CreationDate,
                    x.File
                }).ToList();
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
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
