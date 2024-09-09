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
        private readonly IRepository<CourseSchedule> _courseScheduleRepository;
        private readonly ILogger<CourseScheduleController> _logger;
        private readonly IMapper _mapper;
        public CourseScheduleController( IRepository<CourseSchedule> courseScheduleRepository, IMapper mapper,
            ILogger<CourseScheduleController> logger)
        {
         
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

        [HttpGet("daysOfWeek")]
        public IActionResult GetDaysOfWeek()
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

                var courseScheduleEntity = await _courseScheduleRepository.GetById(id);
                if (courseScheduleEntity == null)
                {
                    return NotFound();
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
                bool courses = await _courseScheduleRepository.DataExist(
                                singlePredicate: x => x.CourseID == courseSchedMdl.CourseID &&
                                x.DayOfWeek == courseSchedMdl.DayOfWeek);

                if (!courses)
                {
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
                    return NotFound("This day is already scheduled for this course");
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
