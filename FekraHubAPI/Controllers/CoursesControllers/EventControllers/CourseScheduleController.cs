using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CourseScheduleController : ControllerBase
    {
        private readonly IRepository<CourseSchedule> _courseScheduleRepository;
   
        private readonly IMapper _mapper;
        public CourseScheduleController( IRepository<CourseSchedule> courseScheduleRepository, IMapper mapper)
        {
         
            _courseScheduleRepository = courseScheduleRepository;
            _mapper = mapper;
        }

        [Authorize(Policy = "ManageCourseSchedule")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_CourseSchedule>>> GetCourseSchedules()
        {

            IQueryable<CourseSchedule> courseSched = (await _courseScheduleRepository.GetRelation<CourseSchedule>());
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
            var courseSched = await _courseScheduleRepository.GetById(id);
            if (courseSched == null)
            {
                return NotFound();
            }
            return Ok(courseSched);
        }


        // PUT: api/CourseSchedule/5
        [Authorize(Policy = "ManageCourseSchedule")]
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
        [Authorize(Policy = "ManageCourseSchedule")]
        [HttpPost]
        public async Task<ActionResult<CourseSchedule>> PostCourseSchedule([FromForm] Map_CourseSchedule courseSchedMdl)
        {

            IQueryable<CourseSchedule> courses = (await _courseScheduleRepository.GetRelation<CourseSchedule>(x => x.CourseID == courseSchedMdl.CourseID))
                .Where(x => x.DayOfWeek == courseSchedMdl.DayOfWeek);


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (courses == null)
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
                return CreatedAtAction("GetCourseSchedule", new { id = courseScheduleEntity.Id }, courseScheduleEntity);

            }
            else
            {
                return NotFound("This day is already scheduled for this course");
            }


        }


        [Authorize(Policy = "ManageCourseSchedule")]
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
