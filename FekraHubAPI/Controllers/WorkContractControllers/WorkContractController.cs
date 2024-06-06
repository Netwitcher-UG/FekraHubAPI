using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using FekraHubAPI.Services;
using Microsoft.AspNetCore.Http;
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
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public WorkContractController(IRepository<WorkContract> workContractRepository, IMapper mapper , UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _workContractRepository = workContractRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult> UploadWorkContract([FromQuery][Required] List<IFormFile> files, [FromQuery][Required] string UserID)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (! WorkContractService.CheckCanDoWorkContract(userRole))
            {
                return BadRequest("Cant Upload Work Contract");
            }

            var user = await _userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                return NotFound();
            }
            var isTeacher = await _userManager.IsInRoleAsync(user, DefaultRole.Teacher);

            if (!isTeacher)
            {
                return BadRequest("User Must Have Teacher Role");
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
                        TeacherID = user.Id,
                    };

                    /*var UploadWorkContractEntity = _mapper.Map<WorkContract>(UploadWorkContract);

                    await _workContractRepository.Add(UploadWorkContractEntity);*/

                }
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkContract(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (!WorkContractService.CheckCanDoWorkContract(userRole))
            {
                 return BadRequest("Cant Delete Work Contract");
            }
            var WorkContractEntity = await _workContractRepository.GetById(id);
            if (WorkContractEntity == null)
            {
                return NotFound();
            }

            await _workContractRepository.Delete(id);
            return NoContent();
        }
       
    }
}
