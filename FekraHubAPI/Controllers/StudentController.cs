using AutoMapper;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        
        [HttpGet("courses/capacity")]
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

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        [HttpPost("student/add")]
        public async Task<IActionResult> AddStudent([FromForm] Map_Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var studentEntity = _mapper.Map<Student>(student);
                await _studentRepo.Add(studentEntity);

                var newStudent = (await _studentRepo.GetAll())
                    .SingleOrDefault(x => x.ParentID == student.ParentID
                                          && x.FirstName == student.FirstName
                                          && x.LastName == student.LastName);

                if (newStudent == null)
                {
                    return NotFound("Student not found after creation.");
                }

                string contract = await _contractMaker.ContractHtml(newStudent.Id);
                return Content(contract);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new student record");
            }
        }
        [HttpPost("student/acceptedContract/{studentId}")]
        public async Task<IActionResult> AcceptedContract(int studentId)
        {
            try
            {
                var contracts = await _studentContractRepo.GetAll();
                if (contracts.Any(c => c.StudentID == studentId))
                {
                    return BadRequest("This student already has a contract!");
                }

                await _contractMaker.ConverterHtmlToPdf(studentId);
                var res = await _emailSender.SendContractEmail(studentId, "Son_Contract");
                if (res is BadRequestObjectResult)
                {
                    await _emailSender.SendContractEmail(studentId, "Son_Contract");
                }

                return Ok("Fekra Hub welcomes your son in our family . A copy of the contract was sent to your email");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("student/notAcceptedContract/{studentId}")]
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
                return BadRequest("This student was not found");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("student/getContracts")]
        public async Task<IActionResult> GetContracts()
        {
            try
            {
                var contracts = await _studentContractRepo.GetAll();
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("student/GetSonsOfParentContracts")]
        public async Task<IActionResult> GetSonsOfParentContracts(string parentId)
        {
            try
            {
                var allContracts = await _studentContractRepo.GetRelation();
                var contracts = allContracts.Include(x => x.Student)
                                            .Where(x => x.Student.ParentID == parentId)
                                            .Select(x => x.File)
                                            .ToList();
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
