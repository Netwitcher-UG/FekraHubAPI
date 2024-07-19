using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.UploadControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadTypeController : ControllerBase
    {
        private readonly IRepository<UploadType> _uploadTypeRepository;
        private readonly IMapper _mapper;

        private readonly ILogger<UploadTypeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public UploadTypeController(IRepository<UploadType> uploadTypeRepository, IMapper mapper, ILogger<UploadTypeController> logger)
        {
            _uploadTypeRepository = uploadTypeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/UploadTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_UploadType>>> GetUploadTypes()
        {
            try
            {
                var uploadType = await _uploadTypeRepository.GetAll();
                return Ok(uploadType);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "UploadTypeController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


        // GET: api/UploadTypes/5

        [HttpGet("{id}")]
        public async Task<ActionResult<Map_UploadType>> GetUploadType(int id)
        {
            try
            {
                var uploadType = await _uploadTypeRepository.GetById(id);
                if (uploadType == null)
                {
                    return NotFound();
                }
                return Ok(uploadType);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "UploadTypeController", ex.Message));
                return BadRequest(ex.Message);
            }


        }


        // PUT: api/UploadTypes/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUploadType(int id, [FromForm] Map_UploadType uploadTypeMdl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var uploadTypeEntity = await _uploadTypeRepository.GetById(id);
                if (uploadTypeEntity == null)
                {
                    return NotFound();
                }

                _mapper.Map(uploadTypeMdl, uploadTypeEntity);
                await _uploadTypeRepository.Update(uploadTypeEntity);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "UploadTypeController", ex.Message));
                return BadRequest(ex.Message);
            }


        }
        // POST: api/UploadTypes
        [HttpPost]
        public async Task<ActionResult<UploadType>> PostUploadType([FromForm] Map_UploadType uploadType)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var uploadTypeEntity = _mapper.Map<UploadType>(uploadType);
                await _uploadTypeRepository.Add(uploadTypeEntity);
                return CreatedAtAction("GetUploadType", new { id = uploadTypeEntity.Id }, uploadTypeEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "UploadTypeController", ex.Message));
                return BadRequest(ex.Message);
            }



        }


        // DELETE: api/UploadTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUploadType(int id)
        {
            try
            {
                var uploadType = await _uploadTypeRepository.GetById(id);
                if (uploadType == null)
                {
                    return NotFound();
                }

                await _uploadTypeRepository.Delete(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "UploadTypeController", ex.Message));
                return BadRequest(ex.Message);
            }


        }

    }
}
