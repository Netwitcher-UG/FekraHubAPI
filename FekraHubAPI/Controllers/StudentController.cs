using AutoMapper;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        public StudentController(IRepository<StudentContract> studentContractRepo, IContractMaker contractMaker,
            IRepository<Student> studentRepo, IRepository<Course> courseRepo, IEmailSender emailSender, IMapper mapper)
        {
            _studentContractRepo = studentContractRepo;
            _contractMaker = contractMaker;
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
            _emailSender = emailSender;
            _mapper = mapper;
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
                var courseInfo = courses.Select(x => new { x.Id , x.Name , x.Capacity}).ToList();
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
                course = new
                {
                    x.CourseID,
                    x.Course.Name,
                    x.Course.Capacity,
                    startDate = x.Course.StartDate.Date,
                    EndDate = x.Course.EndDate.Date,
                    x.Course.Price
                },
                parent = new { x.ParentID, x.User.FirstName, x.User.LastName, x.User.Email }
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
                course = new { z.Course.Id, z.Course.Name, z.Course.Capacity,startDate = z.Course.StartDate.Date, EndDate = z.Course.EndDate.Date, z.Course.Price }
            }).ToList();
            

            return Ok(result);
        }
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> InsertStudent([FromForm] Map_Student student)
        {
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
                await _studentRepo.Add(studentEntity);

                var newStudentID = studentEntity.Id;

                if (newStudentID == 0)
                {
                    var newStudent = (await _studentRepo.GetAll())
                    .SingleOrDefault(x => x.ParentID == student.ParentID
                                          && x.FirstName == student.FirstName
                                          && x.LastName == student.LastName);
                    if(newStudent != null) 
                    {
                        newStudentID = newStudent.Id;
                    }
                    
                }

                List<string> contract = await _contractMaker.ContractHtml(newStudentID);
                return Ok(contract);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating new student record {ex}");
            }
        }
        [HttpPost("AcceptedContract/{studentId}")]
        public async Task<IActionResult> AcceptedContract(int studentId)
        {
            try
            {
                if(!await _studentRepo.IDExists(studentId))
                {
                    return BadRequest("This student not found");
                }
                var contracts = await _studentContractRepo.GetAll();
                if (contracts.Any(c => c.StudentID == studentId))
                {
                    return BadRequest("This student already has a contract!");
                }
                //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //if (string.IsNullOrEmpty(userId))
                //{
                //    return Unauthorized("User ID not found in token.");
                //}
                //student.ParentID = userId;
                await _contractMaker.ConverterHtmlToPdf(studentId);//
                var res = await _emailSender.SendContractEmail(studentId, "Son_Contract");//
                if (res is BadRequestObjectResult)
                {
                    await _emailSender.SendContractEmail(studentId, "Son_Contract");
                }

                return Ok("welcomes your son to our family . A copy of the contract was sent to your email");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("NotAcceptedContract/{studentId}")]
        public async Task<IActionResult> NotAcceptedContract(int studentId)
        {
            try
            {
                var contracts = await _studentContractRepo.GetAll();
                if (contracts.Any(c => c.StudentID == studentId))
                {
                    return BadRequest("This student has a contract!");
                }

                var student = await _studentRepo.GetById(studentId);
                if (student != null)
                {
                    await _studentRepo.Delete(studentId);
                    return Ok($"The student ({student.FirstName} {student.LastName}) has been removed successfully");
                }
                return BadRequest("This student not found");
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
                    ParentFirstName =x.Student.User.FirstName,
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
                    x.Id ,
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
