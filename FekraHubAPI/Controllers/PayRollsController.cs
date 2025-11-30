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
        [HttpPost("fix-names")]
        public async Task<IActionResult> FixNames()
        {
            var payrolls = await _payRollRepository.GetRelationList(
                where:x=>x.Name == null,
                selector:x=>x
                );
            foreach (var item in payrolls)
            {
                item.Name = $"Gehlatnachweis - {item.Timestamp.Month}.{item.Timestamp.Year}";
            }
            await _payRollRepository.ManyUpdate(payrolls);
            return Ok(payrolls.Select(x=>x.Name));
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
                    return BadRequest("Benutzer nicht gefunden.");//User not found.
                }
                //var payrollsExists = await _payRollRepository.DataExist(x=> x.UserID == UserID && x.Timestamp.Month == DateTime.UtcNow.Month);
                //if (payrollsExists)
                //{
                //    return BadRequest("Sie haben diesen Monat eine Gehaltsabrechnung.");//You have a payrolls in this month
                //}
                var isTeacher = await _payRollRepository.IsTeacherIDExists(user.Id);
                var isSecretariat = await _payRollRepository.IsSecretariatIDExists(user.Id);
                if (!(isTeacher || isSecretariat))
                {
                    return BadRequest("Benutzer muss die Rolle Lehrer oder Sekretär haben.");//User Must Have Teacher Or Secrtaria Role
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
                   

                    var UploadPayRoll = new PayRoll
                    {
                        Name = file.FileName,
                        File = filePayRoll,
                        UserID = user.Id,

                    };

                    await _payRollRepository.Add(UploadPayRoll);


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
                    return BadRequest("Datei nicht gefunden.");//File not found
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
            var isSecretariat = await _payRollRepository.IsSecretariatIDExists(id);
            if (!isTeacher && !isSecretariat)
            {
                return BadRequest("The Id does not belong to a teacher or a secretariat");
            }
            var teacherPayrolls = await _payRollRepository.GetRelationList(
                where: x => x.UserID == id,
                asNoTracking: true,
                include: x => x,
                selector: x => new
                {
                    x.Id,
                    x.Timestamp,
                    x.Name
                   
                }
                );
            return Ok(new { Teacher = new { Teacher.Id, Teacher.FirstName, Teacher.LastName }, teacherPayrolls });
        }
        [Authorize(Policy = "ManagePayrolls")]
        [HttpGet("payroll-all")]
        public async Task<IActionResult> GetPayRolls(string id)
        {
            var Teacher = await _userManager.FindByIdAsync(id);
            if (Teacher == null)
            {
                return BadRequest("user not found");
            }
            var isTeacher = await _payRollRepository.IsTeacherIDExists(id);
            var isSecretariat = await _payRollRepository.IsSecretariatIDExists(id);
            if (!isTeacher && !isSecretariat)
            {
                return BadRequest("The Id does not belong to a teacher or secretariat");
            }
            
            var payrolls = await _payRollRepository.GetRelationList(
                where: x => x.UserID == id,
                asNoTracking: true,
                selector: x => new
                {
                    x.Id,
                    x.Timestamp,
                    x.Name
                },
                orderBy:x=>x.Timestamp
                );
            return Ok(new { employee = new { Teacher.Id, Teacher.FirstName, Teacher.LastName }, payrolls });
        }
        [Authorize]
        [HttpGet("payroll-teacher")]
        public async Task<IActionResult> GetMyTeacherPayRolls()
        {
            var userId = _payRollRepository.GetUserIDFromToken(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Das Token enthält keine gültige Benutzer-ID.");// Token لا يحتوي على Id صالح.
            }

            var teacher = await _userManager.FindByIdAsync(userId);
            if (teacher == null)
            {
                return BadRequest("Lehrkraft wurde nicht gefunden.");// Teacher not found.
            }

            var isTeacher = await _payRollRepository.IsTeacher(teacher);
            if (!isTeacher)
            {
                return Forbid("Das aktuelle Konto ist keine Lehrkraft.");// الحساب الحالي ليس أستاذاً.
            }

            var teacherPayrolls = await _payRollRepository.GetRelationList(
                where: x => x.UserID == userId,
                asNoTracking: true,
                selector: x => new
                {
                    x.Id,
                    x.Timestamp,
                    x.Name
                },
                orderBy: x => x.Timestamp
            );

            return Ok(teacherPayrolls);
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
