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

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CourseScheduleController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<CourseSchedule> _courseScheduleRepository;
        private readonly ILogger<CourseScheduleController> _logger;
        private readonly IMapper _mapper;
        public CourseScheduleController( IRepository<CourseSchedule> courseScheduleRepository,
            IRepository<Course> courseRepository, IMapper mapper,
            ILogger<CourseScheduleController> logger)
        {
            _courseRepository = courseRepository;
            _courseScheduleRepository = courseScheduleRepository;
            _logger = logger;
            _mapper = mapper;
        }

        [Authorize(Policy = "ManageCourseSchedule")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_CourseSchedule>>> GetCourseSchedules()
        {
            try
            {
                var result = await _courseScheduleRepository.GetRelationList(
                    include:x=>x.Include(z=>z.Course),
                    selector: z => new
                    {
                        z.Id,
                        z.DayOfWeek,
                        z.StartTime,
                        z.EndTime,
                        courseName = z.Course.Name,
                        courseID = z.Course.Id
                    },
                    asNoTracking: true);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        private string[] Days()
        {
            var daysOfWeek = new[]
         {
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday",
                 "Sunday"
            };


            return daysOfWeek ;
        }

        [HttpGet("daysOfWeek")]
        public IActionResult GetDaysOfWeek()
        {
            var daysOfWeek = Days();
            return Ok(daysOfWeek);

        }



        // GET: api/CourseSchedule/5
        [Authorize(Policy = "ManageCourseSchedule")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_CourseSchedule>> GetCourseSchedule(int id)
        {
            try
            {
                var courseSched = await _courseScheduleRepository.GetById(id);
                if (courseSched == null)
                {
                    return NotFound();
                }
                return Ok(courseSched);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }


        // PUT: api/CourseSchedule/5
        [Authorize(Policy = "ManageCourseSchedule")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseSchedule(int id, [FromForm] Map_CourseSchedule courseSchedMdl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var course= await _courseRepository.GetById(courseSchedMdl.CourseID ?? 0);
                if (course == null)
                {
                    return BadRequest("course not found");
                }

                bool courses = await _courseScheduleRepository.DataExist(
                         singlePredicate: x => x.CourseID == courseSchedMdl.CourseID &&
                         x.DayOfWeek == courseSchedMdl.DayOfWeek && (

                         (TimeSpan.Parse(courseSchedMdl.StartTime) >= x.StartTime &&
                         TimeSpan.Parse(courseSchedMdl.StartTime) <= x.EndTime) ||

                         (TimeSpan.Parse(courseSchedMdl.EndTime) <= x.EndTime &&
                         TimeSpan.Parse(courseSchedMdl.EndTime) >= x.StartTime) ||

                         TimeSpan.Parse(courseSchedMdl.EndTime) == x.EndTime ||
                         TimeSpan.Parse(courseSchedMdl.StartTime) == x.StartTime)
                         );
                if (courses)
                {
                    return BadRequest("This schedule is already scheduled for this course");
                }

                var daysWeek = Days();
                if (!daysWeek.Contains(courseSchedMdl.DayOfWeek))
                {
                    return BadRequest("This day is not correct");
                }

                var courseScheduleEntity = await _courseScheduleRepository.GetById(id);
                if (courseScheduleEntity == null)
                {
                    return BadRequest("course Schedule not found");
                }

                _mapper.Map(courseSchedMdl, courseScheduleEntity);
                await _courseScheduleRepository.Update(courseScheduleEntity);

                return Ok(new
                {
                    courseScheduleEntity.Id,
                    courseScheduleEntity.DayOfWeek,
                    courseScheduleEntity.StartTime,
                    courseScheduleEntity.EndTime,
                    Course = courseScheduleEntity.Course == null ? null : new { courseScheduleEntity.Course.Id, courseScheduleEntity.Course.Name },
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageCourseSchedule")]
        [HttpPost]
        public async Task<ActionResult<CourseSchedule>> PostCourseSchedule([FromForm] Map_CourseSchedule courseSchedMdl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var course = await _courseRepository.GetById(courseSchedMdl.CourseID ?? 0);
                if (course == null)
                {
                    return BadRequest("course not found");
                }

                bool courses = await _courseScheduleRepository.DataExist(
                                singlePredicate: x => x.CourseID == courseSchedMdl.CourseID &&
                                x.DayOfWeek == courseSchedMdl.DayOfWeek &&(

                                (TimeSpan.Parse(courseSchedMdl.StartTime) >= x.StartTime &&
                                TimeSpan.Parse(courseSchedMdl.StartTime) <= x.EndTime) ||
                                
                                (TimeSpan.Parse(courseSchedMdl.EndTime) <= x.EndTime &&
                                TimeSpan.Parse(courseSchedMdl.EndTime) >= x.StartTime) ||

                                TimeSpan.Parse(courseSchedMdl.EndTime) == x.EndTime ||
                                TimeSpan.Parse(courseSchedMdl.StartTime) == x.StartTime)
                                );

                if (!courses)
                {
                    var daysWeek = Days();
                    if (!daysWeek.Contains(courseSchedMdl.DayOfWeek))
                    {
                        return BadRequest("This day is not correct");
                    }
                    
                    var courseSchedule = new CourseSchedule
                    {
                        DayOfWeek = courseSchedMdl.DayOfWeek,
                        StartTime = TimeSpan.Parse(courseSchedMdl.StartTime),
                        EndTime = TimeSpan.Parse(courseSchedMdl.EndTime),
                        CourseID = courseSchedMdl.CourseID
                    };

                    var courseScheduleEntity = _mapper.Map<CourseSchedule>(courseSchedule);
                    await _courseScheduleRepository.Add(courseScheduleEntity);

                    return Ok(new
                    {
                        courseSchedule.Id,
                        courseSchedule.DayOfWeek,
                        courseSchedule.StartTime,
                        courseSchedule.EndTime,
                        Course = courseSchedule.Course == null ? null : 
                        new { courseSchedule.Course.Id, courseSchedule.Course.Name },
                    });
                }
                else
                {
                    return BadRequest("This schedule is already scheduled for this course");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
            


        }


        [Authorize(Policy = "ManageCourseSchedule")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseSchedule(int id)
        {
            try
            {
                var eventType = await _courseScheduleRepository.DataExist(x => x.Id == id);
                if (!eventType)
                {
                    return NotFound();
                }

                await _courseScheduleRepository.Delete(id);

                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
    }
}
