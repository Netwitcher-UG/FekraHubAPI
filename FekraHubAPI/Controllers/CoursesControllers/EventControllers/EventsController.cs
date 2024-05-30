using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<CourseSchedule> _ScheduleRepository;
        private readonly IRepository<CourseEvent> _CourseEventRepository;
        private readonly IMapper _mapper;
        public EventsController(IRepository<Event> eventRepository, IRepository<CourseEvent> CourseEventRepository
           , IMapper mapper ,
            IRepository<CourseSchedule> ScheduleRepository)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _ScheduleRepository = ScheduleRepository;
            _CourseEventRepository  = CourseEventRepository;
        }

        // GET: api/Event
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Event>>> GetEvents()
        {
            var eventE = await _eventRepository.GetAll();

            return Ok(eventE);
        }


        // GET: api/Event/5

        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Event>> GetEvent(int id)
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
        public async Task<IActionResult> PutEvent(int id, [FromForm] Map_Event eventMdl)
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

        [HttpPost("{scheduleId}/Event")]
        public async Task<ActionResult<Event>> PostEvent(int scheduleId, [FromForm] Map_Event eventMdl)
        {
            var schedule = await _ScheduleRepository.GetById(scheduleId);
            if (schedule == null)
            {
                return NotFound("Course schedule not found or does not belong to the course.");
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var eventEntity = _mapper.Map<Event>(eventMdl);
            await _eventRepository.Add(eventEntity);


         


            var courseEvent = new CourseEvent
            {
                ScheduleID = schedule.Id,
                EventID = eventEntity.Id
            };

            var courseEventEntity = _mapper.Map<CourseEvent>(courseEvent);
            await _CourseEventRepository.Add(courseEventEntity);

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
