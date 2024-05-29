using AutoMapper;
using FekraHubAPI.@class.Courses;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesController
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IMapper _mapper;

        public CoursesController(IRepository<Course> courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Course>>> GetCourses()
        {
            var courses = await _courseRepository.GetAll();
       
            return Ok(courses);
        }

        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Course>> GetCourse(int id)
        {
            var course = await _courseRepository.GetById(id);
            if (course == null)
            {
                return NotFound();
            }
                  return Ok(course);
        }

        // POST: api/Course
        [HttpPost]
        public async Task<ActionResult<Map_Course>> PostCourse([FromForm] Map_Course courseMdl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var courseEntity = _mapper.Map<Course>(courseMdl);
            await _courseRepository.Add(courseEntity);

            return CreatedAtAction("GetCourses", new { id = courseEntity.Id }, courseEntity);
        }

        // PUT: api/Course/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, [FromForm] Map_Course courseMdl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var courseEntity = await _courseRepository.GetById(id);
            if (courseEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(courseMdl, courseEntity);
            await _courseRepository.Update(courseEntity);

            return NoContent();
        }

        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var courseEntity = await _courseRepository.GetById(id);
            if (courseEntity == null)
            {
                return NotFound();
            }

            await _courseRepository.Delete(id);
            return NoContent();
        }

    }
}
