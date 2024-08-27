using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
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

        [Authorize(Policy = "ManageEventTypes")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_EventType>>> GetEventTypes()
        {
            var eventType = await _eventTypeRepository.GetAll();

            return Ok(eventType.Select(x => new
            {
                x.Id,
                x.TypeTitle
            }));
        }


        // GET: api/EventType/5
        [Authorize(Policy = "ManageEventTypes")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_EventType>> GetEventType(int id)
        {
            var eventType = await _eventTypeRepository.GetById(id);
            if (eventType == null)
            {
                return NotFound();
            }
            return Ok(new { eventType.Id,eventType.TypeTitle });
        }


        // PUT: api/EventType/5
        [Authorize(Policy = "ManageEventTypes")]
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

            return Ok(new { eventTypeEntity.Id, eventTypeEntity.TypeTitle });
        }
        [Authorize(Policy = "ManageEventTypes")]
        [HttpPost]
        public async Task<ActionResult<EventType>> PostEventType([FromForm] Map_EventType eventType)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var eventTypeEntity = _mapper.Map<EventType>(eventType);
            await _eventTypeRepository.Add(eventTypeEntity);
            return Ok(new { eventTypeEntity.Id, eventTypeEntity.TypeTitle });

        }


        [Authorize(Policy = "ManageEventTypes")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            var eventType = await _eventTypeRepository.GetById(id);
            if (eventType == null)
            {
                return NotFound();
            }

            await _eventTypeRepository.Delete(id);

            return Ok("Delete success");
        }
    }
}
