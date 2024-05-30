using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Models.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesController.EventController
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypeController : ControllerBase
    {
        private readonly IRepository<EventType> _eventTypeRepository;
        private readonly IMapper _mapper;
        public EventTypeController(IRepository<EventType> eventTypeRepository, IMapper mapper)
        {
            _eventTypeRepository = eventTypeRepository;
            _mapper = mapper;
        }

        // GET: api/EventType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_EventType>>> GetEventTypes()
        {
            var eventType = await _eventTypeRepository.GetAll();

            return Ok(eventType);
        }


        // GET: api/EventType/5

        [HttpGet("{id}")]
        public async Task<ActionResult<Map_EventType>> GetEventType(int id)
        {
            var eventType = await _eventTypeRepository.GetById(id);
            if (eventType == null)
            {
                return NotFound();
            }
            return Ok(eventType);
        }


        // PUT: api/EventType/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEventType(int id, [FromForm] Map_EventType eventTypeMdl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventTypeEntity = await _eventTypeRepository.GetById(id);
            if (eventTypeEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(eventTypeMdl, eventTypeEntity);
            await _eventTypeRepository.Update(eventTypeEntity);

            return NoContent();
        }
        // POST: api/EventType
        [HttpPost]
        public async Task<ActionResult<EventType>> PostEventType([FromForm] Map_EventType eventType)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var eventTypeEntity = _mapper.Map<EventType>(eventType);
            await _eventTypeRepository.Add(eventTypeEntity);
            return CreatedAtAction("GetEventType", new { id = eventTypeEntity.Id }, eventTypeEntity);

        }


        // DELETE: api/EventType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            var eventType = await _eventTypeRepository.GetById(id);
            if (eventType == null)
            {
                return NotFound();
            }

            await _eventTypeRepository.Delete(id);

            return NoContent();
        }
    }
}
