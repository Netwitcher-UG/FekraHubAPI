using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Models.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesController.UploadController
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<mdl_UploadType>>> GetUploadTypes()
        {
            var uploadType = await _uploadTypeRepository.GetAll();

            return Ok(uploadType);
        }


        // GET: api/UploadTypes/5

        [HttpGet("{id}")]
        public async Task<ActionResult<mdl_UploadType>> GetUploadType(int id)
        {
            var location = await _uploadTypeRepository.GetById(id);
            if (location == null)
            {
                return NotFound();
            }
            return Ok(location);
        }


        // PUT: api/UploadTypes/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUploadType(int id, [FromForm] mdl_UploadType uploadTypeMdl)
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
        [HttpPost]
        public async Task<ActionResult<UploadType>> PostUploadType([FromForm] mdl_UploadType uploadType)
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUploadType(int id)
        {
            var location = await _uploadTypeRepository.GetById(id);
            if (location == null)
            {
                return NotFound();
            }

            await _uploadTypeRepository.Delete(id);

            return NoContent();
        }

    }
}
