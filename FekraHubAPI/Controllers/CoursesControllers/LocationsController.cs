using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

        private readonly ILogger<LocationsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);


        public LocationsController(IRepository<Location> locationRepository, IMapper mapper, ILogger<LocationsController> logger)
        {

            _locationRepository = locationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_location>>> GetLocations()
        {
            try
            {
                var locations = await _locationRepository.GetAll();

                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


        // GET: api/Locations/5
   
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_location>> GetLocation(int id)
        {
            try
            {
                var location = await _locationRepository.GetById(id);
                if (location == null)
                {
                    return NotFound();
                }
                return Ok(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


        // PUT: api/Locations/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, [FromForm] Map_location locationMdl)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation([FromForm] Map_location location)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var locationEntity = _mapper.Map<Location>(location);
                await _locationRepository.Add(locationEntity);
                return CreatedAtAction("GetLocation", new { id = locationEntity.Id }, locationEntity);

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }



        }


        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var location = await _locationRepository.GetById(id);
                if (location == null)
                {
                    return NotFound();
                }

                await _locationRepository.Delete(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }


        }

    }
}
