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
using System.Linq.Expressions;
using System;


namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<CourseSchedule> _ScheduleRepository;
        private readonly ILogger<AuthorizationUsersController> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        public EventsController(IRepository<Event> eventRepository,
            IRepository<Course> courseRepository
           , IMapper mapper,
            IRepository<CourseSchedule> ScheduleRepository,
            ILogger<AuthorizationUsersController> logger, IEmailSender emailSender)
        {
            _courseRepository = courseRepository;
            _eventRepository = eventRepository;
            _mapper = mapper;
            _ScheduleRepository = ScheduleRepository;
            _ScheduleRepository = ScheduleRepository;
            _logger = logger;
            _emailSender = emailSender;
        }

        // GET: api/Event
        [Authorize(Policy = "GetEvents")]
        [HttpGet]
        public async Task<IActionResult> GetEvents([FromQuery] List<int>? courseId)
        {
            try
            {
                var courses = await _courseRepository.GetRelationList(
                   manyWhere: new List<Expression<Func<Course, bool>>?>
                {
            courseId != null  && courseId.Any() ? (Expression<Func<Course, bool>>)(x => courseId.Contains(x.Id) ) : null,
                  }.Where(x => x != null).Cast<Expression<Func<Course, bool>>>().ToList(),
                    include: x => x.Include(c => c.CourseSchedule),
                   selector: sa => new
                   {
                       id = sa.Id,
                       name = sa.Name,
                       startDate = sa.StartDate,
                       endDate = sa.EndDate,
                       CourseSchedule = sa.CourseSchedule.Select(z => new
                       {
                           z.Id,
                           z.DayOfWeek,
                           z.StartTime,
                           z.EndTime,
                       })
                   }, asNoTracking: true);





                var CourseWorkingDay = courseId == null || courseId.Count == 0 ? null : await _ScheduleRepository.GetRelationList(
                        where: x => courseId.Contains(x.Course.Id),
                        selector: x => x.Id,
                        asNoTracking: true);
                var eventE = await _eventRepository.GetRelationList(
                    where: courseId == null || courseId.Count == 0 ? null : x => x.CourseSchedule.Any(z => CourseWorkingDay.Contains(z.Id)),
                    include: x => x.Include(z => z.EventType).Include(c => c.CourseSchedule),
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


                return Ok(new { Events = eventE, Courses = courses });
               
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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var serverDate = DateTime.Now.Date;
                if (eventMdl.StartDate.Date < serverDate || eventMdl.EndDate.Date < serverDate)
                {
                    return BadRequest("Das Start- oder Enddatum muss nach dem aktuellen Datum liegen.");
                }

                List<CourseSchedule> schedule;

                
                if (scheduleId == null || !scheduleId.Any())
                {
                    var startDate = eventMdl.StartDate.Date;
                    var endDate = eventMdl.EndDate.Date;

                  
                    var daysList = new List<string>();
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        daysList.Add(date.DayOfWeek.ToString());
                    }

                    schedule = await _ScheduleRepository.GetRelationList(
                        where: n => daysList.Contains(n.DayOfWeek)
                                    && eventMdl.StartDate.Date >= n.Course.StartDate.Date
                                    && eventMdl.EndDate.Date <= n.Course.EndDate.Date,
                        selector: x => x
                    );
                }
                else
                {

                    var courseSchedule = await _courseRepository.GetRelationList(where: x => scheduleId.Contains(x.Id),
                        selector: x => x.CourseSchedule.ToList());
                    var mergedCourseSchedules = courseSchedule.SelectMany(cs => cs).ToList();
                    if (courseSchedule == null || !courseSchedule.Any())
                    {
                        return BadRequest("Es wurden keine Kurspläne gefunden.");
                    }
                    schedule = mergedCourseSchedules;
                }

                var courseIds = schedule.Select(z => z.CourseID).Distinct().ToList();
                var courseValid = await _courseRepository.GetRelationList(
                    where: x => courseIds.Contains(x.Id)
                                && eventMdl.StartDate.Date >= x.StartDate.Date
                                && eventMdl.EndDate.Date <= x.EndDate.Date,
                    selector: x => x
                );

                bool isValid = courseValid.Count() == courseIds.Count();
                if (!isValid)
                {
                    return BadRequest("Überprüfen Sie das Start- oder Enddatum der Kurse.");
                }

                Event? eventEntity = await _eventRepository.GetRelationSingle(
                    where: n => n.Id == id,
                    returnType: QueryReturnType.SingleOrDefault,
                    include: x => x.Include(e => e.CourseSchedule),
                    selector: x => x
                );

                if (eventEntity == null)
                {
                    return BadRequest();
                }

              
                var existingSchedules = eventEntity.CourseSchedule.ToList();
                foreach (var cs in existingSchedules)
                {
                    if (cs.Course == null || eventMdl.StartDate.Date < cs.Course.StartDate.Date || eventMdl.EndDate.Date > cs.Course.EndDate.Date)
                    {
                        eventEntity.CourseSchedule.Remove(cs);
                    }

                }

              
                var newSchedules = schedule.Where(cs => !eventEntity.CourseSchedule.Any(es => es.Id == cs.Id)).ToList();
                foreach (var schedule2 in newSchedules)
                {
                    eventEntity.CourseSchedule.Add(schedule2);
                }


         
                eventEntity.EventName = eventMdl.EventName;
                eventEntity.Description = eventMdl.Description;
                eventEntity.StartDate = eventMdl.StartDate;
                eventEntity.EndDate = eventMdl.EndDate;
                eventEntity.StartTime = new TimeSpan(eventMdl.StartDate.Hour, eventMdl.StartDate.Minute, 0);
                eventEntity.EndTime = new TimeSpan(eventMdl.EndDate.Hour, eventMdl.EndDate.Minute, 0);
                eventEntity.TypeID = eventMdl.TypeID;

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
                    CourseSchedule = eventEntity.CourseSchedule.Select(x => new
                    {
                        x.Id,
                        x.DayOfWeek,
                        x.StartTime,
                        x.EndTime,
                        x.CourseID
                    })
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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (eventMdl.StartDate.Date > eventMdl.EndDate.Date)
                {
                    return BadRequest("Ungültiges Veranstaltungsdatum.");
                }

                var serverDate = DateTime.Now.Date;
                if (eventMdl.StartDate.Date < serverDate || eventMdl.EndDate.Date < serverDate)
                {
                    return BadRequest("Das Start- oder Enddatum muss nach dem aktuellen Datum liegen.");
                }

                List<CourseSchedule> schedule = new List<CourseSchedule>();

                
                if (scheduleId == null || !scheduleId.Any())
                {
                    var startDate = eventMdl.StartDate.Date;
                    var endDate = eventMdl.EndDate.Date;

                 
                    var daysList = new List<string>();
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        daysList.Add(date.DayOfWeek.ToString());
                    }

                    schedule = await _ScheduleRepository.GetRelationList(
                        where: n => daysList.Contains(n.DayOfWeek)
                                    && eventMdl.StartDate.Date >= n.Course.StartDate.Date
                                    && eventMdl.EndDate.Date <= n.Course.EndDate.Date,
                        selector: x => x
                    );
                }
                else
                {
                    var courseSchedule = await _courseRepository.GetRelationList(where:x => scheduleId.Contains(x.Id),
                        selector:x=>x.CourseSchedule.ToList());
                    var mergedCourseSchedules = courseSchedule.SelectMany(cs => cs).ToList();
                    if (courseSchedule == null || !courseSchedule.Any())
                    {
                        return BadRequest("Es wurden keine Kurspläne gefunden.");
                    }
                    schedule = mergedCourseSchedules;
                }

              
                var courseIds = schedule.Select(z => z.CourseID).Distinct().ToList();
                var courseValid = await _courseRepository.GetRelationList(
                    where: x => courseIds.Contains(x.Id)
                                && eventMdl.StartDate.Date >= x.StartDate.Date
                                && eventMdl.EndDate.Date <= x.EndDate.Date,
                    selector: x => x
                );

                bool isValid = courseValid.Count() == courseIds.Count();
                if (!isValid)
                {
                    return BadRequest("Überprüfen Sie das Start- oder Enddatum der Kurse.");
                }

              
                var startTime = new TimeSpan(eventMdl.StartDate.Hour, eventMdl.StartDate.Minute, 0);
                var endTime = new TimeSpan(eventMdl.EndDate.Hour, eventMdl.EndDate.Minute, 0);

              
                var eventEntity = new Event
                {
                    EventName = eventMdl.EventName,
                    Description = eventMdl.Description,
                    StartDate = eventMdl.StartDate,
                    EndDate = eventMdl.EndDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    TypeID = eventMdl.TypeID
                };

              
                var relatedSchedules = schedule.Where(cs =>
                    eventMdl.StartDate.Date >= cs.Course.StartDate.Date &&
                    eventMdl.EndDate.Date <= cs.Course.EndDate.Date
                ).ToList();

               
                if (relatedSchedules.Any())
                {
                    eventEntity.CourseSchedule = relatedSchedules;
                }

                await _eventRepository.Add(eventEntity);

                var courses = relatedSchedules.Select(x => x.CourseID).ToList();
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
                    CourseSchedule = eventEntity.CourseSchedule?.Select(x => new
                    {
                        x.Id,
                        x.DayOfWeek,
                        x.StartTime,
                        x.EndTime,
                        x.CourseID
                    })
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
                    return BadRequest("Veranstaltung nicht gefunden.");//Event not found
                }

                await _eventRepository.Delete(id);

                return Ok("Erfolgreich gelöscht");//Deleted success
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventsController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
    }
}
