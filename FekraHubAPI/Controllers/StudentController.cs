using AutoMapper;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                var courseInfo = courses.Select(x => new { x.Id, x.Name, x.Capacity }).ToList();
                return Ok(courseInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var Allstudents = await _studentRepo.GetRelation();
            var students = Allstudents.Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.Birthday,
                x.Nationality,
                x.Note,
                city = x.City ?? "Like parent",
                Street = x.Street ?? "Like parent",
                StreetNr = x.StreetNr ?? "Like parent",
                ZipCode = x.ZipCode ?? "Like parent",
                course = new
                {
                    x.CourseID,
                    x.Course.Name,
                    x.Course.Capacity,
                    startDate = x.Course.StartDate.Date,
                    EndDate = x.Course.EndDate.Date,
                    x.Course.Price
                },
                parent = new { x.ParentID, x.User.FirstName, x.User.LastName, x.User.Email, x.User.City, x.User.Street, x.User.StreetNr, x.User.ZipCode }
            }).ToList();
            return Ok(students);
        }
        [HttpGet("ByParent")]
        //[Authorize]
        public async Task<IActionResult> GetStudentsByParent(string parentId)//
        {
            //var parentId =_courseRepo.GetUserIDFromToken(User);

            //if (string.IsNullOrEmpty(parentId))
            //{
            //    return Unauthorized("Parent ID not found in token.");
            //}

            var students = (await _studentRepo.GetRelation()).Where(x => x.ParentID == parentId);

            var result = students.Select(z => new
            {
                z.Id,
                z.FirstName,
                z.LastName,
                z.Birthday,
                z.Nationality,
                z.Note,
                city = z.City ?? "Like parent",
                Street = z.Street ?? "Like parent",
                StreetNr = z.StreetNr ?? "Like parent",
                ZipCode = z.ZipCode ?? "Like parent",
                course = new { z.Course.Id, z.Course.Name, z.Course.Capacity, startDate = z.Course.StartDate.Date, EndDate = z.Course.EndDate.Date, z.Course.Price }
            }).ToList();


            return Ok(result);
        }
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> GetContract([FromForm] Map_Student student)
        {
            var par = _userManager.Users.FirstOrDefault();
            student.ParentID = par.Id;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //if (string.IsNullOrEmpty(userId))
                //{
                //    return Unauthorized("User ID not found in token.");
                //}
                //student.ParentID = userId;

                var studentEntity = _mapper.Map<Student>(student);
                List<string> contract = await _contractMaker.ContractHtml(studentEntity, student.ParentID);
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
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrEmpty(userId))
            //{
            //    return Unauthorized("User ID not found in token.");
            //}
            //student.ParentID = userId;
            var par = _userManager.Users.FirstOrDefault();
            student.ParentID = par.Id;
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var studentEntity = _mapper.Map<Student>(student);
                await _studentRepo.Add(studentEntity);
                await _contractMaker.ConverterHtmlToPdf(studentEntity);//
                if (!(await _studentContractRepo.GetRelation()).Where(x => x.StudentID == studentEntity.Id).Any())
                {
                    return BadRequest("something is wrong please try again");
                }
                var res = await _emailSender.SendContractEmail(studentEntity.Id, "Son_Contract");//
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
                var contracts = await _studentContractRepo.GetRelation();
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
        public async Task<IActionResult> GetSonsOfParentContracts(string parentId)// (string parentId) delete when [Authorize]
        {
            try
            {
                //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //if (string.IsNullOrEmpty(userId))
                //{
                //    return Unauthorized("User ID not found in token.");
                //}
                //var parentId = userId;
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

    }
}
