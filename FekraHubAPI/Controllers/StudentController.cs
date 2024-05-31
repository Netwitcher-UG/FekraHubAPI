using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
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
        public StudentController(IRepository<StudentContract> studentContractRepo, IContractMaker contractMaker,
            IRepository<Student> studentRepo, IRepository<Course> courseRepo, IEmailSender emailSender)
        {
            _studentContractRepo = studentContractRepo;
            _contractMaker = contractMaker;
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
            _emailSender = emailSender;
        }
        
        [HttpGet("/courses/capacityCourse")]
        public async Task<IActionResult> ChooseCourse()
        {
            var courses = await _courseRepo.GetAll();
            var allstudentsInCourses = await _studentRepo.GetAll();
            foreach (var course in courses)
            {
                course.Capacity = course.Capacity - allstudentsInCourses.Where(c => c.CourseID == course.Id).Count();
            }
            return Ok(courses);
        }
        [HttpPost("/student/add")]
        public async Task<IActionResult> CreateStudent(Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _studentRepo.Add(student);
            string contract = await _contractMaker.ContractHtml(student.Id);
            return Content(contract);
        }
        [HttpPost("/student/acceptedContract/{studentId}")]
        public async Task<IActionResult> AcceptedContract(int studentId)
        {
            var contracts = await _studentContractRepo.GetAll();
            var isExists = contracts.Where(c => c.StudentID == studentId).Any();
            if (isExists)
            {
                return BadRequest("This student already has a contract!");
            }
            await _contractMaker.ConverterHtmlToPdf(studentId);
            var res = await _emailSender.SendContractEmail(studentId, "Son_Contract");
            if (res is BadRequestObjectResult)
            {
                await _emailSender.SendContractEmail(studentId, "Son_Contract");
            }
            return Ok();
        }
        [HttpGet("/student/getContracts")]
        public async Task<IActionResult> GetContracts()
        {
            var contracts = await _studentContractRepo.GetAll();
            return Ok(contracts);
        }
        [HttpGet("/student/GetSonsOfParentContracts")]
        public async Task<IActionResult> GetSonsOfParentContracts(string parentId)
        {
            var AllContracts = await _studentContractRepo.GetRelation();
            var constracts = AllContracts.Include(x => x.Student).Where(x => x.Student.ParentID == parentId).ToListAsync();
            return Ok(constracts);
        }
    }
}
