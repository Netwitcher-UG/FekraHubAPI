using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly IRepository<ApplicationUser> _teacherRepository;
        private readonly IMapper _mapper;

        private readonly ILogger<CoursesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public CoursesController(IRepository<Course> courseRepository,
              IRepository<ApplicationUser> teacherRepository,
            IRepository<Student> studentRepository, IMapper mapper, ILogger<CoursesController> logger)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Course>>> GetCourses(string? search)
        {
            try
            {

                IQueryable<Course> courses = await _courseRepository.GetRelation();

                if (search != null)
                {
                    courses = courses.Where(x => x.Name.Contains(search));
                }

                var result = await courses.Select(sa => new
                {
                    id = sa.Id,
                    name = sa.Name,
                    price = sa.Price,
                    lessons = sa.Lessons,
                    capacity = sa.Capacity,
                    startDate = sa.StartDate,
                    endDate = sa.EndDate,
                    Room = new { sa.Room.Id, sa.Room.Name },
                    Teacher = sa.Teacher.Select(z => new
                    {
                        z.Id,
                        z.FirstName,
                        z.LastName
                    })



                }).ToListAsync();


                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Course>> GetCourse(int id)
        {
            try
            {
                var course = await _courseRepository.GetById(id);
                if (course == null)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
  
        }

        // POST: api/Course
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse([FromForm] string[] TeacherId, [FromForm] Map_Course courseMdl)
        {
            try
            {
                var x = (await _teacherRepository.GetRelation()).Where(n => TeacherId.Contains(n.Id)).ToList();

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var courseEntity = new Course
                {
                    Name = courseMdl.Name,
                    Price = courseMdl.Price,
                    Lessons = courseMdl.Lessons,
                    Capacity = courseMdl.Capacity,
                    StartDate = courseMdl.StartDate,
                    EndDate = courseMdl.EndDate,
                    RoomId = courseMdl.RoomId,
                    Teacher = new List<ApplicationUser>()
                };

                courseEntity.Teacher = x;

                await _courseRepository.Add(courseEntity);




                return CreatedAtAction("GetCourses", new { id = courseEntity.Id }, courseEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        // PUT: api/Course/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, [FromForm] string[] TeacherId, [FromForm] Map_Course courseMdl)
        {
            try
            {
                var Teacher = (await _teacherRepository.GetRelation()).Where(n => TeacherId.Contains(n.Id)).ToList();


                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var courseEntity = (await _courseRepository.GetRelation()).Where(n => n.Id == id)
                  .Include(e => e.Teacher).First();

                if (courseEntity == null)
                {
                    return NotFound();
                }

                courseEntity.Teacher.Clear();


                courseEntity.Teacher = Teacher;

                _mapper.Map(courseMdl, courseEntity);
                await _courseRepository.Update(courseEntity);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var courseEntity = await _courseRepository.GetById(id);
                if (courseEntity == null)
                {
                    return NotFound();
                }

                await _courseRepository.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


        [HttpPost("AssignStudentsToCourse")]
        public async Task<IActionResult> AssignStudentsToCourse( int courseID, [FromBody] List<int> studentIds)
        {
            try
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
                var students = (await _studentRepository.GetRelation())
                                               .Where(s => studentIds.Contains(s.Id));

                if (!students.Any())
                {
                    return NotFound("No students found with the provided IDs");
                }

                await students.ForEachAsync(student => student.CourseID = courseID);
                await _studentRepository.ManyUpdate(students);


                return NoContent(); // HTTP 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }

        }



    }
}
