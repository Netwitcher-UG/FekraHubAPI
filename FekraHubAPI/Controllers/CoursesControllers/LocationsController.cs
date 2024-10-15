using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var locations = await _locationRepository.GetRelationList(
                    selector: x => new { x.Id, x.Name },
                    orderBy:x=>x.Id,
                    asNoTracking:true);
                if (locations == null)
                {
                    return BadRequest("Keine Standorte gefunden.");//no locations found
                }
                return Ok(locations);
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
                var locations = await _locationRepository.GetRelationList(
                where: search != null ? x =>
                x.Name.Contains(search) ||
                x.Street.Contains(search) || 
                x.StreetNr.Contains(search) ||
                x.ZipCode.Contains(search) ||
                x.City.Contains(search) : null,
                include:x=>x.Include(z=>z.room),
                orderBy:x=>x.Id,
                selector: x => new 
                {
                    x.Id,
                    x.Name,
                    x.City,
                    x.Street,
                    x.StreetNr,
                    x.ZipCode,
                    Room = x.room.Select(z => new { 
                        z.Id,
                        z.Name
                    }) 
                },
                asNoTracking:true);

                return Ok(locations);
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
                var location = await _locationRepository.GetRelationSingle(
                    where:x=>x.Id == id,
                    include:x=>x.Include(z=>z.room),
                    returnType:QueryReturnType.SingleOrDefault,
                    selector: x => new
                    {
                        x.Id,
                        x.Name,
                        x.City,
                        x.Street,
                        x.ZipCode,
                        x.StreetNr,
                        room = x.room == null ? null : x.room.Select(z=>new
                        {
                            z.Id,
                            z.Name
                        })
                        
                    },
                    asNoTracking:true
                    );
                if (location == null )
                {
                    return BadRequest("Standort nicht gefunden.");//Location not found
                }
                return Ok(location);

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        public class Map_RoomLocation
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class LocationRoomDTO
        {
            public int Id { get; set; }
            public Map_location locationMdl { get; set; }
            public List<Map_RoomLocation> rooms { get; set; }
        }
        //====================================================================== later
        // PUT: api/Locations/5
        [Authorize(Policy = "ManageLocations")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation([FromBody] LocationRoomDTO locationRoomDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var locationEntity = await _locationRepository.GetRelationSingle(
                     where: x => x.Id == locationRoomDTO.Id,
                    include: x => x.Include(z => z.room),
                    returnType: QueryReturnType.SingleOrDefault,
                    selector: x => x
                    );
                
                if (locationEntity == null)
                {
                    return BadRequest("Standort nicht gefunden.");//Location not found
                }

                _mapper.Map(locationRoomDTO.locationMdl, locationEntity);
                await _locationRepository.Update(locationEntity);

                var oldRooms = await _roomRepository.GetRelationList(
                    where: x=> locationRoomDTO.rooms.Select(z=>z.Id).Contains(x.Id),
                    selector:x=>x
                    );
                foreach (var newRoom in locationRoomDTO.rooms)
                {
                    var oldRoom = oldRooms.FirstOrDefault(x => x.Id == newRoom.Id);

                    if (oldRoom != null)
                    {
                        oldRoom.Name = newRoom.Name;
                    }
                }
                await _roomRepository.ManyUpdate(oldRooms);

                return Ok(new
                {
                    locationEntity.Id,
                    locationEntity.Name,
                    locationEntity.City,
                    locationEntity.Street,
                    locationEntity.StreetNr,
                    locationEntity.ZipCode,
                    Room = locationEntity.room.Select(r => new
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

        // DELETE: api/Locations/5
        [Authorize(Policy = "ManageLocations")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try 
            {
                var location = await _locationRepository.DataExist(x=>x.Id == id);
                if (!location)
                {
                    return BadRequest("Standort nicht gefunden.");//Location not found
                }
                var roomExist = await _roomRepository.DataExist(n => n.LocationID == id);
                if (roomExist)
                {
                    return BadRequest("Dieser Standort enthält Räume!!");//This Location contains Rooms !!
                }

                await _locationRepository.Delete(id);

                return Ok("Erfolgreich gelöscht");//Deleted success

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "LocationsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

    }
}
