using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.AuthorizationController;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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
        private readonly IEmailSender _emailSender;
        public EventsController(IRepository<Event> eventRepository
           , IMapper mapper,
            IRepository<CourseSchedule> ScheduleRepository,
            ILogger<AuthorizationUsersController> logger, IEmailSender emailSender)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _ScheduleRepository = ScheduleRepository;
            _logger = logger;
            _emailSender = emailSender;
        }

        // GET: api/Event
        [Authorize(Policy = "GetEvents")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Event>>> GetEvents([FromQuery] List<int>? courseId)
        {
            try
            {

                var CourseWorkingDay = courseId == null || courseId.Count == 0 ? null : await _ScheduleRepository.GetRelationList(
                        where: x => courseId.Contains(x.Course.Id),
                        selector: x => x.Id,
                        asNoTracking: true);
                var eventE = await _eventRepository.GetRelationList(
                    where: courseId == null || courseId.Count == 0 ? null : x => x.CourseSchedule.Any(z => CourseWorkingDay.Contains(z.Id)),
                    include:x=> x.Include(z=>z.EventType).Include(c=>c.CourseSchedule),
                selector: x => new
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


                },
                asNoTracking: true);


                return Ok(eventE);
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
                var eventEntity = await _eventRepository.GetRelationSingle(
                    where:x => x.Id == id,
                    returnType:QueryReturnType.SingleOrDefault,
                    include:x=>x.Include(e=>e.EventType).Include(c=>c.CourseSchedule),
                    selector: eventEntity => new
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
                    },
                    asNoTracking: true);
                if (eventEntity == null)
                {
                    return NotFound();
                }

                return Ok(eventEntity);
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
                var schedule = await _ScheduleRepository.GetRelationList(
                    where:n => scheduleId.Contains(n.Id),
                    selector: x=>x,
                    asNoTracking: true);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            Event? eventEntity = await _eventRepository.GetRelationSingle(
                where: n => n.Id == id,
                returnType:QueryReturnType.SingleOrDefault,
                include:x=>x.Include(e => e.CourseSchedule),
                selector:x=>x);

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
                var schedule = await _ScheduleRepository.GetRelationList(
                    where:n => scheduleId.Contains(n.Id),
                    selector:x=>x,
                    asNoTracking:true);

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
                    StartTime = eventMdl.StartDate.TimeOfDay,
                    EndTime = eventMdl.EndDate.TimeOfDay,
                    TypeID = eventMdl.TypeID,
                    CourseSchedule = schedule

                };

                await _eventRepository.Add(eventEntity);
                var courses = schedule.Select(x => x.CourseID).ToList();
                if (courses.Any())
                {
                    await _emailSender.SendToAllNewEvent(courses);
                }
                
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
                    CourseSchedule = eventEntity.CourseSchedule.Select(x => new 
                    { x.Id, x.DayOfWeek, x.StartTime, x.EndTime, x.CourseID })
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
                var eventType = await _eventRepository.DataExist(x=>x.Id == id);
                if (!eventType)
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
