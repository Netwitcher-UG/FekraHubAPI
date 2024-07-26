using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolInfoController : ControllerBase
    {
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly IMapper _mapper;


        private readonly ILogger<SchoolInfoController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public SchoolInfoController(IRepository<SchoolInfo> schoolInfoRepo,IMapper mapper, ILogger<SchoolInfoController> logger)
        {
            _schoolInfoRepo = schoolInfoRepo;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSchoolInfo()
        {
            try
            {
                var schoolInfo = (await _schoolInfoRepo.GetAll()).FirstOrDefault();
                if (schoolInfo == null)
                {
                    return NotFound();
                }
                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> InsertSchoolInfo([FromForm] Map_SchoolInfo SchoolInfo)
        {
            try
            {
                var schoolInfos = await _schoolInfoRepo.GetAll();
                if (schoolInfos.Any())
                {
                    return BadRequest("School Information was added earlier");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(SchoolInfo);
                }
                var schoolInfo = _mapper.Map<SchoolInfo>(SchoolInfo);
                await _schoolInfoRepo.Add(schoolInfo);
                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [HttpPut]
        public async Task<IActionResult> UpdateSchoolInfo([FromForm] Map_SchoolInfo schoolInfo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(schoolInfo);
                }
                var schoolInfos = (await _schoolInfoRepo.GetRelation()).FirstOrDefault();
                if (schoolInfos == null)
                {
                    return BadRequest("No school Information added");
                }
                _mapper.Map(schoolInfo, schoolInfos);
                await _schoolInfoRepo.Update(schoolInfos);
                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [HttpDelete]
        public async Task<IActionResult> DeleteSchoolInfo()
        {
            try
            {
                var schoolInfos = await _schoolInfoRepo.GetAll();
                foreach (var schoolInfo in schoolInfos)
                {
                    await _schoolInfoRepo.Delete(schoolInfo.Id);
                }
                return Ok("Done");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
    }
}
