using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly IMapper _mapper;

        public CoursesController(IRepository<Course> courseRepository, IRepository<Student> studentRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Course>>> GetCourses()
        {
            IQueryable<Course> courses = await _courseRepository.GetRelation();

            var result = await courses.Select(sa => new
            {
                name = sa.Name,
                price = sa.Price,
                lessons = sa.Lessons,
                capacity = sa.Capacity,
                startDate = sa.StartDate,
                endDate = sa.EndDate,
                Room = new { sa.Room.Id, sa.Room.Name }


            }).ToListAsync();
        

            return Ok(result);
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


        [HttpPost("AssignStudentsToCourse")]
        public async Task<IActionResult> AssignStudentsToCourse( int courseID, [FromBody] List<int> studentIds)
        {
            if (courseID <= 0 || studentIds == null || !studentIds.Any())
            {
                return BadRequest("Invalid course ID or student list");
            }

            var course = await _courseRepository.GetById(courseID);
            if (course == null)
            {
                return NotFound("Course not found");
            }
            var students =( await _studentRepository.GetRelation())
                                           .Where(s => studentIds.Contains(s.Id));

            if (!students.Any())
            {
                return NotFound("No students found with the provided IDs");
            }

            await students.ForEachAsync(student => student.CourseID = courseID);
            await _studentRepository.ManyUpdate(students);


            return NoContent(); // HTTP 204 No Content
        }



    }
}
