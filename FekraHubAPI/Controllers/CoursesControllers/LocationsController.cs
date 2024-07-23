using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<Map_location>>> GetLocations(string? search)

        {
            var locations = await _locationRepository.GetRelation();
            if (search != null) 
            {
                locations = locations.Where(x => x.Name.Contains(search) || x.Street.Contains(search) || x.StreetNr.Contains(search)
                || x.ZipCode.Contains(search) || x.City.Contains(search)).OrderByDescending(x => x.Id);
            }
            else
            {
                locations = locations.OrderByDescending(x => x.Id);
            }
            return Ok(locations.Select(x => new {x.Id,x.Name,x.City,x.Street,x.StreetNr,x.ZipCode,Room = x.room.Select(z => new { z.Id,z.Name })  }));
        }


        // GET: api/Locations/5
   
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_location>> GetLocation(int id)
        {
            var location = await _locationRepository.GetById(id);
            if (location == null)
            {
                return NotFound();
            }
            return Ok(new { location.Id,location.Name, location.City, location.Street, location.StreetNr, location.ZipCode });
        }


        // PUT: api/Locations/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, [FromForm] Map_location locationMdl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var locationEntity = await _locationRepository.GetById(id);
            if (locationEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(locationMdl, locationEntity);
            await _locationRepository.Update(locationEntity);

            return NoContent();
        }
        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation([FromForm] Map_location location)
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
