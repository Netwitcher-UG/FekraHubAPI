using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [Authorize(Policy = "ManageRoom")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Room>>> GetRooms()
        {
            try
            {
                IQueryable<Room> rooms = (await _roomRepository.GetRelation<Room>());
                var result = rooms.Select(x => new
                {
                    x.Id,
                    x.Name,
                    locationId = x.Location.Id,
                    locationName = x.Location.Name,
                    locationStreet = x.Location.Street,
                    locationStreetNr = x.Location.StreetNr,
                    locationZipCode = x.Location.ZipCode,
                    locationCity = x.Location.City,


                }).ToList();
                return Ok(result);
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
                    return NotFound();
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
                    return NotFound();
                }

                _mapper.Map(room, roomEntity);
                await _roomRepository.Update(roomEntity);

                return NoContent();
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
                var roomEntity = _mapper.Map<Room>(room);
                await _roomRepository.Add(roomEntity);
                return CreatedAtAction("GetRoom", new { id = roomEntity.Id }, roomEntity);
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
                var room = await _roomRepository.GetById(id);
                if (room == null)
                {
                    return NotFound();
                }
                var CourseExist = (await _CourseRepository.GetRelation<Course>(n => n.RoomId == id)).Any();
                if (CourseExist)
                {
                    return BadRequest("This room contains Courses !!");
                }

                await _roomRepository.Delete(id);

                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "RoomsController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
    }
}
    
