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
    public class EventsController : ControllerBase
    {
        private readonly IRepository<Event> _eventRepository;
        private readonly IMapper _mapper;
        public EventsController(IRepository<Event> eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        // GET: api/Event
        [HttpGet]
        public async Task<ActionResult<IEnumerable<mdl_Event>>> GetEvents()
        {
            var eventE = await _eventRepository.GetAll();

            return Ok(eventE);
        }


        // GET: api/Event/5

        [HttpGet("{id}")]
        public async Task<ActionResult<mdl_Event>> GetEvent(int id)
        {
            var eventE = await _eventRepository.GetById(id);
            if (eventE == null)
            {
                return NotFound();
            }
            return Ok(eventE);
        }


        // PUT: api/Event/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromForm] mdl_Event eventMdl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventEntity = await _eventRepository.GetById(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(eventMdl, eventEntity);
            await _eventRepository.Update(eventEntity);

            return NoContent();
        }
        // POST: api/Event
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent([FromForm] mdl_Event eventMdl)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var eventEntity = _mapper.Map<Event>(eventMdl);
            await _eventRepository.Add(eventEntity);
            return CreatedAtAction("GetEvent", new { id = eventEntity.Id }, eventEntity);

        }


        // DELETE: api/Event/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventType = await _eventRepository.GetById(id);
            if (eventType == null)
            {
                return NotFound();
            }

            await _eventRepository.Delete(id);

            return NoContent();
        }
    }
}
