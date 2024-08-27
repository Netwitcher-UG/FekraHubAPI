using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.AuthorizationController;
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
        private readonly ILogger<AuthorizationUsersController> _logger;
        private readonly IMapper _mapper;
        public EventsController(IRepository<Event> eventRepository
           , IMapper mapper,
            IRepository<CourseSchedule> ScheduleRepository,
            ILogger<AuthorizationUsersController> logger)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _ScheduleRepository = ScheduleRepository;
            _logger = logger;
        }

        // GET: api/Event
        [Authorize(Policy = "GetEvents")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Event>>> GetEvents(List<int>? courseId)
        {
            try
            {
                
                IQueryable<Event> eventE = await _eventRepository.GetRelation<Event>();
                if(courseId != null)
                {
                    var CourseWorkingDay = (await _ScheduleRepository.GetRelation<int>(
                        x => courseId.Contains( x.Course.Id),null, x => x.Id)).ToList();
                    eventE = eventE.Where(x =>  x.CourseSchedule.Any(z => CourseWorkingDay.Contains(z.Id)));
                }
                var result = eventE.Select(x => new
                {
                    x.Id,
                    x.EventName,
                    x.Description,
                    x.StartDate,
                    x.EndDate,
                    x.StartTime,
                    x.EndTime,
                    EventType = x.EventType == null ? null : new
                    {
                        x.EventType.Id,
                        x.EventType.TypeTitle
                    },
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }


        // GET: api/Event/5

        [Authorize(Policy = "GetEvents")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Event>> GetEvent(int id)
        {
            try
            {
                var eventEntity = (await _eventRepository.GetRelation<Event>(x => x.Id == id));
                if (eventEntity.SingleOrDefault() == null)
                {
                    return NotFound();
                }

                return Ok(eventEntity.Select(eventEntity => new
                {
                    eventEntity.Id,
                    eventEntity.EventName,
                    eventEntity.Description,
                    eventEntity.StartDate,
                    eventEntity.StartTime,
                    eventEntity.EndDate,
                    eventEntity.EndTime,
                    EventType = eventEntity.EventType == null ? null : new
                    {
                        eventEntity.EventType.Id,
                        eventEntity.EventType.TypeTitle
                    },
                    CourseSchedule = eventEntity.CourseSchedule == null ? null : eventEntity.CourseSchedule.Select(x => new { x.Id, x.DayOfWeek, x.StartTime, x.EndTime, x.CourseID })
                }).SingleOrDefault());
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }


        // PUT: api/Event/5

        [Authorize(Policy = "ManageEvents")]

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromForm] int[] scheduleId, [FromForm] Map_Event eventMdl)
        {
            try
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


                return Ok(new
                {
                    eventEntity.Id,
                    eventEntity.EventName,
                    eventEntity.Description,
                    eventEntity.StartDate,
                    eventEntity.StartTime,
                    eventEntity.EndDate,
                    eventEntity.EndTime,
                    eventEntity.TypeID,
                    CourseSchedule = eventEntity.CourseSchedule.Select(x => new { x.Id, x.DayOfWeek, x.StartTime, x.EndTime, x.CourseID })
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        // POST: api/Event
        [Authorize(Policy = "ManageEvents")]
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent([FromForm] int[] scheduleId, [FromForm] Map_Event eventMdl)
        {
            try
            {
                var schedule = (await _ScheduleRepository.GetRelation<CourseSchedule>(n => scheduleId.Contains(n.Id))).ToList();

             


                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var eventEntity = new Event
                {
                    EventName = eventMdl.EventName,
                    Description = eventMdl.Description,
                    StartDate = eventMdl.StartDate,
                    EndDate = eventMdl.EndDate,
                    StartTime = TimeSpan.Parse(eventMdl.StartDate.ToString("HH:mm:ss")),
                    EndTime = TimeSpan.Parse(eventMdl.EndDate.ToString("HH:mm:ss")),
                    TypeID = eventMdl.TypeID,
                    CourseSchedule = new List<CourseSchedule>()

                };

                eventEntity.CourseSchedule = schedule;


                await _eventRepository.Add(eventEntity);
                return Ok(new
                {
                    eventEntity.Id,
                    eventEntity.EventName,
                    eventEntity.Description,
                    eventEntity.StartDate,
                    eventEntity.StartTime,
                    eventEntity.EndDate,
                    eventEntity.EndTime,
                    eventEntity.TypeID,
                    CourseSchedule = eventEntity.CourseSchedule.Select(x => new { x.Id, x.DayOfWeek, x.StartTime, x.EndTime, x.CourseID })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }


        // DELETE: api/Event/5
        [Authorize(Policy = "ManageEvents")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var eventType = await _eventRepository.GetById(id);
                if (eventType == null)
                {
                    return NotFound();
                }

                await _eventRepository.Delete(id);

                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
    }
}
