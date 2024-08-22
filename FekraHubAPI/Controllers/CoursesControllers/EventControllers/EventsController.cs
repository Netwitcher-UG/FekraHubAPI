using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<CourseSchedule> _ScheduleRepository;

        private readonly IMapper _mapper;
        public EventsController(IRepository<Event> eventRepository
           , IMapper mapper,
            IRepository<CourseSchedule> ScheduleRepository)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _ScheduleRepository = ScheduleRepository;

        }

        // GET: api/Event
        [Authorize(Policy = "GetEvents")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Event>>> GetEvents()
        {

            IQueryable<Event> eventE = await _eventRepository.GetRelation<Event>();

            var result = eventE.Select(x => new
            {
                x.Id,
                x.EventName,
                x.Date,
                x.EventType.TypeTitle,
                CourseSchedule = x.CourseSchedule.Select(z => new
                {
                    z.Id,
                    z.DayOfWeek,
                    z.StartTime,
                    z.EndTime,
                    courseName = z.Course.Name,
                    courseID = z.Course.Id

                })


            }).ToList();

            return Ok(result);
        }


        // GET: api/Event/5

        [Authorize(Policy = "GetEvents")]
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

        [Authorize(Policy = "ManageEvents")]

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromForm] int[] scheduleId, [FromForm] Map_Event eventMdl)
        {
            var schedule = (await _ScheduleRepository.GetRelation<CourseSchedule>(n => scheduleId.Contains(n.Id))).ToList();




            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var eventEntity = (await _eventRepository.GetRelation<Event>(n => n.Id == id))
               .Include(e => e.CourseSchedule).First();

            if (eventEntity == null)
            {
                return NotFound();
            }
            eventEntity.CourseSchedule.Clear();


            eventEntity.CourseSchedule = schedule;


            _mapper.Map(eventMdl, eventEntity);
            await _eventRepository.Update(eventEntity);





            return NoContent();
        }
        // POST: api/Event
        [Authorize(Policy = "ManageEvents")]
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent([FromForm] int[] scheduleId, [FromForm] Map_Event eventMdl)
        {
            var x = (await _ScheduleRepository.GetRelation<CourseSchedule>(n => scheduleId.Contains(n.Id))).ToList();

            //var schedule = await _ScheduleRepository.GetById(scheduleId);
            //if (schedule == null)
            //{
            //    return NotFound("Course schedule not found or does not belong to the course.");
            //}


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var eventEntity = new Event
            {
                EventName = eventMdl.EventName,
                Date = eventMdl.Date,
                TypeID = eventMdl.TypeID,

                CourseSchedule = new List<CourseSchedule>()
            };


            eventEntity.CourseSchedule = x;

            await _eventRepository.Add(eventEntity);

            return CreatedAtAction("GetEvent", new { id = eventEntity.Id }, eventEntity);

        }


        // DELETE: api/Event/5
        [Authorize(Policy = "ManageEvents")]

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
