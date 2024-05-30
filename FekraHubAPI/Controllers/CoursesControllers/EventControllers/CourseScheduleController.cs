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
    public class CourseScheduleController : ControllerBase
    {
           private readonly IRepository<CourseSchedule> _courseScheduleRepository;
        private readonly IMapper _mapper;
        public CourseScheduleController(IRepository<CourseSchedule> courseScheduleRepository, IMapper mapper)
        {
            _courseScheduleRepository = courseScheduleRepository;
            _mapper = mapper;
        }

        // GET: api/CourseSchedule
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_CourseSchedule>>> GetCourseSchedules()
        {
            var courseSched = await _courseScheduleRepository.GetAll();

            return Ok(courseSched);
        }


        // GET: api/CourseSchedule/5

        [HttpGet("{id}")]
        public async Task<ActionResult<Map_CourseSchedule>> GetCourseSchedule(int id)
        {
            var courseSched = await _courseScheduleRepository.GetById(id);
            if (courseSched == null)
            {
                return NotFound();
            }
            return Ok(courseSched);
        }


        // PUT: api/CourseSchedule/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseSchedule(int id, [FromForm] Map_CourseSchedule courseSchedMdl)
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
        // POST: api/CourseSchedule
        [HttpPost]
        public async Task<ActionResult<CourseSchedule>> PostCourseSchedule([FromForm] Map_CourseSchedule courseSchedMdl)
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


        // DELETE: api/CourseSchedule/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseSchedule(int id)
        {
            var eventType = await _courseScheduleRepository.GetById(id);
            if (eventType == null)
            {
                return NotFound();
            }

            await _courseScheduleRepository.Delete(id);

            return NoContent();
        }
    }
}
