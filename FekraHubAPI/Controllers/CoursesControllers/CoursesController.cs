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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<ApplicationUser> _teacherRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Room> _roomRepo;
        public CoursesController(IRepository<Course> courseRepository,
              IRepository<ApplicationUser> teacherRepository,
            IRepository<Student> studentRepository, IMapper mapper, IRepository<Room> roomRepo)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _mapper = mapper;
            _roomRepo = roomRepo;
        }
        [Authorize]
        [HttpGet("GetCoursesName")]
        public async Task<IActionResult> GetCoursesName()
        {
            var userId = _courseRepository.GetUserIDFromToken(User);
            var Teacher = await _courseRepository.IsTeacherIDExists(userId);
            IQueryable<Course> courses = await _courseRepository.GetRelation<Course>();
            if (Teacher)
            {
            courses = courses.Where(z => z.Teacher.Select(n => n.Id).Contains(userId));
            }
                if (courses == null)
                {
                    return NotFound("no course found");
                }
                return Ok(courses.Select(x => new { x.Id, x.Name }));
           

          
        }
        // GET: api/Course
        [Authorize(Policy = "GetCourse")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Course>>> GetCourses(string? search)
        {
            var userId = _courseRepository.GetUserIDFromToken(User);
            var Teacher = await _courseRepository.IsTeacherIDExists(userId);

            IQueryable<Course> courses = await _courseRepository.GetRelation<Course>(search == null ? null : x => x.Name.Contains(search));
            if (Teacher)
            {
                courses = courses.Where(z => z.Teacher.Select(n => n.Id).Contains(userId) );
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
                Room = sa.Room == null ? null : new { sa.Room.Id, sa.Room.Name },
                Location = sa.Room == null || sa.Room.Location == null ? null : new { sa.Room.Location.Id, sa.Room.Location.Name },
                Teacher = sa.Teacher == null ? null : sa.Teacher.Select(z => new
                {
                    z.Id,
                    z.FirstName,
                    z.LastName
                })



            }).ToListAsync();
            return Ok(result);


        }

        [Authorize(Policy = "GetCourse")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Course>> GetCourse(int id)
        {
            var userId = _courseRepository.GetUserIDFromToken(User);
            var Teacher = await _courseRepository.IsTeacherIDExists(userId);
            IQueryable<Course> courses = await _courseRepository.GetRelation<Course>(x => x.Id == id);

            if (Teacher)
            {
               courses = courses.Where(z => z.Teacher.Select(n => n.Id).Contains(userId));
               

            }
           
                if (courses == null)
                {
                    return NotFound();
                }
                return Ok(courses.Select(sa => new
                {
                    id = sa.Id,
                    name = sa.Name,
                    price = sa.Price,
                    lessons = sa.Lessons,
                    capacity = sa.Capacity,
                    startDate = sa.StartDate,
                    endDate = sa.EndDate,
                    Room = sa.Room == null ? null : new { sa.Room.Id, sa.Room.Name },
                    Location = sa.Room == null || sa.Room.Location == null ? null : new { sa.Room.Location.Id, sa.Room.Location.Name },
                    Teacher = sa.Teacher == null ? null : sa.Teacher.Select(z => new
                    {
                        z.Id,
                        z.FirstName,
                        z.LastName
                    })



                }).FirstOrDefault());
            

           
        }

        [Authorize(Policy = "AddCourse")]
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse([FromForm] string[] TeacherId, [FromForm] Map_Course courseMdl)
        {
            if (TeacherId == null || TeacherId.Length == 0)
            {
                return BadRequest("The teacherId is required!!");
            }
            var teachers = (await _teacherRepository.GetRelation<ApplicationUser>(n => TeacherId.Contains(n.Id))).ToList();
            if (!teachers.Any()) 
            {
                return BadRequest("The teacherId does not exist");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var room = await _roomRepo.GetById(courseMdl.RoomId);
            if (room == null)
            {
                return BadRequest("The RoomId does not exist");
            }

            var courseEntity = new Course
            {
                Name        = courseMdl.Name,
                Price      = courseMdl.Price,
                Lessons    = courseMdl.Lessons,
                Capacity   = courseMdl.Capacity,
                StartDate  = courseMdl.StartDate,
                EndDate    = courseMdl.EndDate,
                RoomId     = courseMdl.RoomId,
                Teacher = new List<ApplicationUser>()
            };
            courseEntity.Teacher = teachers;
            await _courseRepository.Add(courseEntity);
            return Ok(new
            {
                id = courseEntity.Id,
                name = courseEntity.Name,
                price = courseEntity.Price,
                lessons = courseEntity.Lessons,
                capacity = courseEntity.Capacity,
                startDate = courseEntity.StartDate,
                endDate = courseEntity.EndDate,
                Room = courseEntity.Room == null ? null : new { room.Id, room.Name },
                Location = courseEntity.Room == null || courseEntity.Room.Location == null ? null : new { courseEntity.Room.Location.Id, courseEntity.Room.Location.Name },
                Teacher = courseEntity.Teacher == null ? null : courseEntity.Teacher.Select(z => new
                {
                    z.Id,
                    z.FirstName,
                    z.LastName
                })
            });
        }

        // PUT: api/Course/5
        [Authorize(Policy = "putCourse")]        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, [FromForm] string[] TeacherId, [FromForm] Map_Course courseMdl)
        {
            var Teacher = (await _teacherRepository.GetRelation<ApplicationUser>(n => TeacherId.Contains(n.Id))).ToList();


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var courseEntity =(await _courseRepository.GetRelation<Course>(n => n.Id == id))
              .Include(e => e.Teacher).First();

            if (courseEntity == null)
            {
                return NotFound();
            }

            courseEntity.Teacher.Clear();


            courseEntity.Teacher = Teacher;

            _mapper.Map(courseMdl, courseEntity);
            await _courseRepository.Update(courseEntity);

            return Ok( new
            {
                id = courseEntity.Id,
                name = courseEntity.Name,
                price = courseEntity.Price,
                lessons = courseEntity.Lessons,
                capacity = courseEntity.Capacity,
                startDate = courseEntity.StartDate,
                endDate = courseEntity.EndDate,
                Room = courseEntity.Room == null ? null : new { courseEntity.Room.Id, courseEntity.Room.Name },
                Location = courseEntity.Room == null || courseEntity.Room.Location == null ? null : new { courseEntity.Room.Location.Id, courseEntity.Room.Location.Name },
                Teacher = courseEntity.Teacher == null ? null : courseEntity.Teacher.Select(z => new
                {
                    z.Id,
                    z.FirstName,
                    z.LastName
                })

            });
        }

        [Authorize(Policy = "DeleteCourse")]
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
                var studentExist = (await _studentRepository.GetRelation<Student>(n => n.CourseID == id)).Any();
                if (studentExist)
                {
                    return BadRequest("This course contains students !!");
                }
                await _courseRepository.Delete(id);
                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Policy = "ManageStudentsToCourses")]
        [HttpPost("AssignStudentsToCourse")]
        public async Task<IActionResult> AssignStudentsToCourse( int courseID, [FromBody] List<int> studentIds)
        {
            try { 
            if (courseID <= 0 || studentIds == null || !studentIds.Any())
            {
                return BadRequest("Invalid course ID or student list");
            }

            var course = await _courseRepository.GetById(courseID);
            if (course == null)
            {
                return NotFound("Course not found");
            }
            var students = await _studentRepository.GetRelation<Student>(s => studentIds.Contains(s.Id));

            if (!students.Any())
            {
                return NotFound("No students found with the provided IDs");
            }

            await students.ForEachAsync(student => student.CourseID = courseID);
            await _studentRepository.ManyUpdate(students);
                return Ok(students.Select(x => new
                {   x.FirstName,
                    x.LastName,
                    x.Course.Id,
                    x.Course.Name
                } ));

            }catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }



    }
}
