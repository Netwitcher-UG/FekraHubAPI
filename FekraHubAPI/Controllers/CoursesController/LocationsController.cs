using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Models.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesController
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IMapper _mapper;
        public LocationsController(IRepository<Location> locationRepository, IMapper mapper)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            var locations = await _locationRepository.GetAll();
            return Ok(locations);
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            var location = await _locationRepository.GetById(id);
            if (location == null)
            {
                return NotFound();
            }
            return Ok(location);
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            try
            {
                await _locationRepository.Update(location);
            }
            catch
            {
                if (await _locationRepository.GetById(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation([FromForm] mdl_location location)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var locationEntity = _mapper.Map<Location>(location);
            await _locationRepository.Add(locationEntity);
            return CreatedAtAction("GetLocation", new { id = locationEntity.Id }, locationEntity);

        }


        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _locationRepository.GetById(id);
            if (location == null)
            {
                return NotFound();
            }

            await _locationRepository.Delete(id);

            return NoContent();
        }
    }
}
