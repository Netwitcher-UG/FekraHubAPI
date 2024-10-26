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
using System.IO;
using System.Reflection.Emit;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<Course> _CourseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoomsController> _logger;
        public RoomsController(IRepository<Room> roomRepository, IRepository<Course> CourseRepository,
            IMapper mapper, ILogger<RoomsController> logger)
        {
            _roomRepository = roomRepository;
            _CourseRepository = CourseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Room>>> GetRooms()
        {
            try
            {
                var rooms = await _roomRepository.GetRelationList(
                    include:x=>x.Include(l=>l.Location),
                    selector: x => new
                    {
                        x.Id,
                        x.Name,
                        locationId = x.Location.Id,
                        locationName = x.Location.Name,
                        locationStreet = x.Location.Street,
                        locationStreetNr = x.Location.StreetNr,
                        locationZipCode = x.Location.ZipCode,
                        locationCity = x.Location.City,
                    },
                    orderBy:x=>x.Name,
                    asNoTracking:true
                    );
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "RoomsController", ex.Message));
                return BadRequest(ex.Message);
            }

            
        }

        // GET: api/Rooms/5
        [Authorize(Policy = "ManageRoom")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Room>> GetRoom(int id)
        {
            try
            {
                var room = await _roomRepository.GetById(id);
                if (room == null)
                {
                    return BadRequest("Raum nicht gefunden.");//Room not found
                }
                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "RoomsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        // PUT: api/Rooms/5
        [Authorize(Policy = "ManageRoom")]
        [HttpPut("{id}")]
   
        public async Task<IActionResult> PutRoom(int id, [FromForm] Map_Room room)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var roomEntity = await _roomRepository.GetById(id);
                if (roomEntity == null)
                {
                    return BadRequest("Raum nicht gefunden.");//Room not found
                }

                _mapper.Map(room, roomEntity);
                await _roomRepository.Update(roomEntity);

                return Ok(new
                {
                    roomEntity.Id,
                    roomEntity.Name,
                    Location = roomEntity.Location == null ? null : new { roomEntity.Location.Id, roomEntity.Location.Name },
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "RoomsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        // POST: api/Rooms

        [Authorize(Policy = "ManageRoom")]
        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom([FromForm] Map_Room room)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var roomEntity = _mapper.Map<Room>(room);
                await _roomRepository.Add(roomEntity);
                return Ok(new
                {
                    roomEntity.Id,
                    roomEntity.Name,
                    Location = roomEntity.Location == null  ? null : new { roomEntity.Location.Id, roomEntity.Location.Name }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "RoomsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        // DELETE: api/Rooms/5
        [Authorize(Policy = "ManageRoom")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try 
            {
                var room = await _roomRepository.DataExist(x=> x.Id == id);
                if (!room)
                {
                    return BadRequest("Raum nicht gefunden.");//Room not found
                }
                var CourseExist = await _CourseRepository.DataExist(n => n.RoomId == id);
                if (CourseExist)
                {
                    return BadRequest("Dieser Raum enthält Kurse!!");//This room contains Courses !!
                }

                await _roomRepository.Delete(id);

                return Ok("Erfolgreich gelöscht");//Deleted success
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "RoomsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
    }
}
    
