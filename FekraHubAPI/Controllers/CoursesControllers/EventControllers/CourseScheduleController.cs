using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.AuthorizationController;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CourseScheduleController : ControllerBase
    {
           private readonly IRepository<CourseSchedule> _courseScheduleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseScheduleController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public CourseScheduleController(IRepository<CourseSchedule> courseScheduleRepository
            , IMapper mapper
            , ILogger<CourseScheduleController> logger
            )
        {
            _courseScheduleRepository = courseScheduleRepository;
            _mapper = mapper;
            _logger = logger;

        }


        // GET: api/CourseSchedule
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_CourseSchedule>>> GetCourseSchedules()
        {
            try
            {
                IQueryable<CourseSchedule> courseSched = (await _courseScheduleRepository.GetRelation());
                var result = courseSched.Select(z => new
                {
             
                        z.Id,
                        z.DayOfWeek,
                        z.StartTime,
                        z.EndTime,
                        courseName = z.Course.Name,
                        courseID = z.Course.Id

                }).ToList();

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
        }


        // GET: api/CourseSchedule/5

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
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


        // PUT: api/CourseSchedule/5

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

                return NoContent();

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        // POST: api/CourseSchedule
        [HttpPost]
        public async Task<ActionResult<CourseSchedule>> PostCourseSchedule([FromForm] Map_CourseSchedule courseSchedMdl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
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
                return CreatedAtAction("GetCourseSchedule", new { id = courseScheduleEntity.Id }, courseScheduleEntity);

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
        }


        // DELETE: api/CourseSchedule/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseSchedule(int id)
        {
            try
            {
                 var eventType = await _courseScheduleRepository.GetById(id);
                if (eventType == null)
                {
                    return NotFound();
                }

                await _courseScheduleRepository.Delete(id);

                return NoContent();

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CourseScheduleController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
    }
}
