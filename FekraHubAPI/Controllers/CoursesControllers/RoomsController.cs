using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRepository<Room> _roomRepository;
        private readonly IMapper _mapper;

        public RoomsController(IRepository<Room> roomRepository, IMapper mapper)
        {
            _roomRepository = roomRepository;
            _mapper = mapper;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Room>>> GetRooms()
        {
            var rooms = await _roomRepository.GetAll();
            return Ok(rooms);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Room>> GetRoom(int id)
        {
            var room = await _roomRepository.GetById(id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
   
        public async Task<IActionResult> PutRoom(int id, [FromForm] Map_Room room)
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

        // POST: api/Rooms


        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom([FromForm] Map_Room room)
        {
            var roomEntity = _mapper.Map<Room>(room);
            await _roomRepository.Add(roomEntity);
            return CreatedAtAction("GetRoom", new { id = roomEntity.Id }, roomEntity);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _roomRepository.GetById(id);
            if (room == null)
            {
                return NotFound();
            }

            await _roomRepository.Delete(id);

            return NoContent();
        }
    }
}
    
