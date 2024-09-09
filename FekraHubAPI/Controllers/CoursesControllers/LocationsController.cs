using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Room> _roomRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LocationsController> _logger;
        public LocationsController(IRepository<Room> roomRepository, IRepository<Location> locationRepository,
            IMapper mapper , ILogger<LocationsController> logger)
        {
            _roomRepository = roomRepository;
            _locationRepository = locationRepository;
            _mapper = mapper;
            _logger = logger;
        }
        [Authorize]
        [HttpGet("GetLocationsNames")]
        public async Task<IActionResult> GetLocationsNames()
        {
            try
            {
                var locations = await _locationRepository.GetRelation<Location>();
                if (locations == null)
                {
                    return NotFound("no locations found");
                }
                return Ok(locations.Select(x => new { x.Id, x.Name }));
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageLocations")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_location>>> GetLocations(string? search)
        {
            try
            {
                var locations = (await _locationRepository.GetRelation<Location>(
                search != null ? x => x.Name.Contains(search) || x.Street.Contains(search) || x.StreetNr.Contains(search)
                || x.ZipCode.Contains(search) || x.City.Contains(search) : null
                )).OrderByDescending(x => x.Id);

                return Ok(locations.Select(x => new { x.Id, x.Name, x.City, x.Street, x.StreetNr, x.ZipCode, Room = x.room.Select(z => new { z.Id, z.Name }) }));
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }


        // GET: api/Locations/5
        [Authorize(Policy = "ManageLocations")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            try
            {
                var location = await _locationRepository.GetById(id);
                if (location == null)
                {
                    return NotFound();
                }
                return Ok(new { location.Id, location.Name, location.City, location.Street, location.StreetNr, location.ZipCode });

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

       
        // PUT: api/Locations/5
        [Authorize(Policy = "ManageLocations")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, [FromForm] Map_location locationMdl, [FromForm] List<string> rooms)
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

                await _roomRepository.DeleteRange(x => x.LocationID == id);
                List<Room> room = new List<Room>();
                foreach (var r in rooms)
                {
                    room.Add(new Room { Name = r, LocationID = locationEntity.Id });
                }
                await _roomRepository.ManyAdd(room);


                return Ok(new
                {
                    locationEntity.Id,
                    locationEntity.Name,
                    locationEntity.City,
                    locationEntity.Street,
                    locationEntity.StreetNr,
                    locationEntity.ZipCode,
                    Room = room.Select(r => new
                    {
                        r.Id,
                        r.Name
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        // POST: api/Locations
        [Authorize(Policy = "ManageLocations")]
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation([FromForm] Map_location location, [FromForm] List<string> rooms)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Location locationEntity = _mapper.Map<Location>(location);
                await _locationRepository.Add(locationEntity);
                List<Room> room = new List<Room>();   
                foreach (var r in rooms)
                {
                    room.Add(new Room { Name = r ,LocationID = locationEntity .Id});
                }
                await _roomRepository.ManyAdd(room);
                return Ok(new
                {
                    locationEntity.Id,
                    locationEntity.Name,
                    locationEntity.City,
                    locationEntity.Street,
                    locationEntity.StreetNr,
                    locationEntity.ZipCode,
                    Room = room.Select(r => new
                    {
                        r.Id,
                        r.Name
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Locations/5
        [Authorize(Policy = "ManageLocations")]
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
                var roomExist = (await _roomRepository.GetRelation<Room>(n => n.LocationID == id)).Any();
                if (roomExist)
                {
                    return BadRequest("This Location contains Rooms !!");
                }

                await _locationRepository.Delete(id);

                return Ok("Delete success");

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

    }
}
