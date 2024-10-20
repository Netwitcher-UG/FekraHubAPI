using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using FekraHubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FekraHubAPI.Controllers.WorkContractControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkContractController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRepository<WorkContract> _workContractRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WorkContractController> _logger;
        public ClaimsPrincipal User => HttpContext?.User!;

        public WorkContractController(IRepository<WorkContract> workContractRepository,
            IMapper mapper , UserManager<ApplicationUser> userManager,
            ILogger<WorkContractController> logger)
        {
            _userManager = userManager;
            _workContractRepository = workContractRepository;
            _mapper = mapper;
            _logger = logger;
        }
        [Authorize(Policy = "ManageWorkContract")]
        [HttpGet("{workContractID}")]
        public async Task<IActionResult> GetWorkContract(int workContractID)
        {
            try
            {
                var WorkContract = await _workContractRepository.GetById(workContractID);
                if (WorkContract == null )
                {
                    return BadRequest("Arbeitsvertrag nicht gefunden.");//WorkContract Not Fount

                }
                var data = new {WorkContract.Id , WorkContract.File ,  WorkContract.TeacherID};
                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "WorkContractController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "ManageWorkContract")]
        [HttpGet("[action]")]

        public async Task<IActionResult> GetMyWorkContract()
        {
            try
            {
                var authId = _workContractRepository.GetUserIDFromToken(User);
                var WorkContractEntity = await _workContractRepository.GetAll();
                var WorkContractUser = WorkContractEntity.Where(i => i.TeacherID == authId);
                var data = WorkContractUser.Select(x => new { x.Id, x.TeacherID,x.FileName, x.File }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "WorkContractController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "ManageWorkContract")]
        [HttpPost]
        public async Task<ActionResult> UploadWorkContract([FromQuery][Required] List<IFormFile> files
            , [FromQuery][Required] string UserID
            )
        {
            try
            {
                var user = await _userManager.FindByIdAsync(UserID);
                if (user == null)
                {
                    return BadRequest("Benutzer nicht gefunden.");//User Not Found
                }
                var isTeacher = await _workContractRepository.IsTeacherIDExists(user.Id);
                var isSecretariat = await _workContractRepository.IsSecretariat(user);


                if (!(isTeacher || isSecretariat))
                {
                    return BadRequest("Benutzer muss die Rolle Lehrer oder Sekretär haben.");//User Must Have Teacher Or Secrtaria Role
                }
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        byte[] fileBytes;
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            fileBytes = ms.ToArray();
                        }
                        var fileWorkContract = fileBytes;
                        var UploadWorkContract = new WorkContract
                        {
                            File = fileWorkContract,
                            Timestamp = DateTime.Now,
                            FileName = file.FileName,
                            TeacherID = UserID,

                        };
                        var workContractEntity = _mapper.Map<WorkContract>(UploadWorkContract);
                        await _workContractRepository.Add(workContractEntity);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "WorkContractController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageWorkContract")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkContract(int id)
        {
            try
            {
                var WorkContractEntity = await _workContractRepository.GetById(id);
                if (WorkContractEntity == null)
                {
                    return BadRequest("Arbeitsvertrag nicht gefunden.");//WorkContract not found
                }
                await _workContractRepository.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "WorkContractController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageWorkContract")]
        [HttpGet("[action]/{userID}")]
        public async Task<IActionResult> GetByUserID(string userID)
        {
            try
            {
                var WorkContractEntity = await _workContractRepository.GetAll();
                var WorkContractUser = WorkContractEntity.Where(i => i.TeacherID == userID);
                var data = WorkContractUser.Select(x => new { x.Id, x.TeacherID, x.FileName, x.File }).ToList();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "WorkContractController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
      

    }
}
