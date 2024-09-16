using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayRollsController : ControllerBase
    {
        private readonly IRepository<PayRoll> _payRollRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<PayRollsController> _logger;
        public PayRollsController(IRepository<PayRoll> payRollRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper, ILogger<PayRollsController> logger)
        {
            _payRollRepository = payRollRepository;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize(Policy = "ManagePayrolls")]
        [HttpPost]
        public async Task<IActionResult> PostpayRoll([FromForm] string UserID, IFormFile file)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(UserID);

                if (user == null)
                {
                    return NotFound("User not found.");
                }
                var payrollsExists = await _payRollRepository.DataExist(x => x.UserID == UserID && x.Timestamp.Month == DateTime.Now.Month);
                if (payrollsExists)
                {
                    return BadRequest("You have a payrolls in this month");
                }
                var isTeacher = await _payRollRepository.IsTeacherIDExists(user.Id);
                var isSecretariat = await _payRollRepository.IsSecretariatIDExists(user.Id);
                if (!(isTeacher || isSecretariat))
                {
                    return BadRequest("User Must Have Teacher Or Secrtaria Role");
                }
                if (file.Length > 0)
                {
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }
                    var filePayRoll = fileBytes;
                    var UploadPayRoll = new Map_PayRoll
                    {
                        File = filePayRoll,
                        UserID = user.Id,

                    };

                    var PayRollEntity = _mapper.Map<PayRoll>(UploadPayRoll);
                    await _payRollRepository.Add(PayRollEntity);


                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "PayRollsController", ex.Message));
                return BadRequest(ex.Message);
            }


        }
        [Authorize(Policy = "ManagePayrolls")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayRoll(int id)
        {
            try
            {
                var PayRollEntity = await _payRollRepository.DataExist(x=>x.Id == id);
                if (!PayRollEntity)
                {
                    return NotFound();
                }

                await _payRollRepository.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "PayRollsController", ex.Message));
                return BadRequest(ex.Message);
            }

            
        }

        [Authorize(Policy = "GetTeacher")]
        [HttpGet("TeacherProfile")]
        public async Task<IActionResult> GetTeacherPayRolls(string id)
        {
            var Teacher = await _userManager.FindByIdAsync(id);
            if (Teacher == null)
            {
                return BadRequest("Teacher not found");
            }
            var isTeacher = await _payRollRepository.IsTeacherIDExists(id);
            if (!isTeacher)
            {
                return BadRequest("The Id does not belong to a teacher");
            }
            var teacherPayrolls = await _payRollRepository.GetRelationList(
                where: x => x.UserID == id,
                asNoTracking: true,
                include: x => x,
                selector: x => new
                {
                    x.Id,
                    x.Timestamp,
                   
                }
                );
            return Ok(new { Teacher = new { Teacher.Id, Teacher.FirstName, Teacher.LastName }, teacherPayrolls });
        }
        [Authorize(Policy = "GetTeacher")]
        [HttpGet("DownloadPayrolls")]
        public async Task<IActionResult> GetDownloadTeacherPayrolls(int id)
        {
            var payRolls = await _payRollRepository.GetById(id);
            if (payRolls == null)
            {
                return BadRequest("File not found");
            }
            return Ok(Convert.ToBase64String(payRolls.File));
        }




    }
}
