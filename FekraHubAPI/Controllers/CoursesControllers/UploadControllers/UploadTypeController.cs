using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.UploadControllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UploadTypeController : ControllerBase
    {
        private readonly IRepository<UploadType> _uploadTypeRepository;
        private readonly IMapper _mapper;
        public UploadTypeController(IRepository<UploadType> uploadTypeRepository, IMapper mapper)
        {
            _uploadTypeRepository = uploadTypeRepository;
            _mapper = mapper;
        }

        // GET: api/UploadTypes
        [Authorize(Policy = "ManageBooks")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_UploadType>>> GetUploadTypes()
        {
            var uploadType = await _uploadTypeRepository.GetAll();

            return Ok(uploadType);
        }


        // GET: api/UploadTypes/5
        [Authorize(Policy = "ManageBooks")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_UploadType>> GetUploadType(int id)
        {
            var uploadType = await _uploadTypeRepository.GetById(id);
            if (uploadType == null)
            {
                return NotFound();
            }
            return Ok(uploadType);
        }


        // PUT: api/UploadTypes/5
        [Authorize(Policy = "ManageBooks")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUploadType(int id, [FromForm] Map_UploadType uploadTypeMdl)
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
        // POST: api/UploadTypes
        [Authorize(Policy = "ManageBooks")]
        [HttpPost]
        public async Task<ActionResult<UploadType>> PostUploadType([FromForm] Map_UploadType uploadType)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uploadTypeEntity = _mapper.Map<UploadType>(uploadType);
            await _uploadTypeRepository.Add(uploadTypeEntity);
            return CreatedAtAction("GetUploadType", new { id = uploadTypeEntity.Id }, uploadTypeEntity);

        }


        // DELETE: api/UploadTypes/5
        [Authorize(Policy = "ManageBooks")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUploadType(int id)
        {
            var uploadType = await _uploadTypeRepository.GetById(id);
            if (uploadType == null)
            {
                return NotFound();
            }

            await _uploadTypeRepository.Delete(id);

            return NoContent();
        }

    }
}
