using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
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
using static FekraHubAPI.Controllers.CoursesControllers.CoursesController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<CourseSchedule> _courseScheduleRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<ApplicationUser> _teacherRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Room> _roomRepo;
        private readonly ILogger<CoursesController> _logger;
        public CoursesController(IRepository<Course> courseRepository,
              IRepository<ApplicationUser> teacherRepository,
            IRepository<Student> studentRepository, IMapper mapper, IRepository<Room> roomRepo,
            ILogger<CoursesController> logger, IRepository<CourseSchedule> courseScheduleRepository)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _mapper = mapper;
            _roomRepo = roomRepo;
            _logger = logger;
            _courseScheduleRepository = courseScheduleRepository;
        }
        [Authorize]
        [HttpGet("GetCoursesName")]
        public async Task<IActionResult> GetCoursesName()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
            
           

          
        }
        // GET: api/Course
        [Authorize(Policy = "GetCourse")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_Course>>> GetCourses(string? search)
        {
            try
            {
                var userId = _courseRepository.GetUserIDFromToken(User);
                var Teacher = await _courseRepository.IsTeacherIDExists(userId);

                IQueryable<Course> courses = await _courseRepository.GetRelation<Course>(search == null ? null : x => x.Name.Contains(search));
                if (Teacher)
                {
                    courses = courses.Where(z => z.Teacher.Select(n => n.Id).Contains(userId));
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
            


        }

        [Authorize(Policy = "GetCourse")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_Course>> GetCourse(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
            
            

           
        }
        public class MapCourseSchedule
        {
            public string[] TeacherId { get; set; }
            public Map_Course course { get; set; }
            public List<Map_CourseSchedule> courseSchedule { get; set; }
        }
        [Authorize(Policy = "AddCourse")]
        [HttpPost]
                public async Task<ActionResult<Course>> PostCourse(MapCourseSchedule mapCourseSchedule)
                {
                    try
                    {
                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }
                        if (mapCourseSchedule.TeacherId == null || mapCourseSchedule.TeacherId.Length == 0)
                        {
                            return BadRequest("The teacherId is required!!");
                        }
                        var teachers = (await _teacherRepository.GetRelation<ApplicationUser>(n => mapCourseSchedule.TeacherId.Contains(n.Id))).ToList();
                        if (!teachers.Any())
                        {
                            return BadRequest("The teacherId does not exist");
                        }


                        var room = (await _roomRepo.GetRelation<Room>(x => x.Id == mapCourseSchedule.course.RoomId))
                                    .Select(x => new { x.Id ,x.Name,Location = new { x.Location.Id, x.Location.Name } })
                                    .SingleOrDefault();
                        if (room == null)
                        {
                            return BadRequest("The RoomId does not exist");
                        }
                        
                        var courseEntity = new Course
                        {
                            Name = mapCourseSchedule.course.Name,
                            Price = mapCourseSchedule.course.Price,
                            Lessons = mapCourseSchedule.course.Lessons,
                            Capacity = mapCourseSchedule.course.Capacity,
                            StartDate = mapCourseSchedule.course.StartDate,
                            EndDate = mapCourseSchedule.course.EndDate,
                            RoomId = mapCourseSchedule.course.RoomId,
                            Teacher = new List<ApplicationUser>()
                        };
                        courseEntity.Teacher = teachers;
                        await _courseRepository.Add(courseEntity);

                        List<CourseSchedule> courseSchedules = new List<CourseSchedule>();
                        foreach(var courseSched in mapCourseSchedule.courseSchedule)
                        {
                            var courseSchedule = new CourseSchedule
                            {
                                DayOfWeek = courseSched.DayOfWeek,
                                StartTime = TimeSpan.Parse(courseSched.StartTime),
                                EndTime = TimeSpan.Parse(courseSched.EndTime),
                                CourseID = courseEntity.Id
                            };
                            courseSchedules.Add(courseSchedule);
                        }

                        await _courseScheduleRepository.ManyAdd(courseSchedules);

                        return Ok(new
                        {
                            id = courseEntity.Id,
                            name = courseEntity.Name,
                            price = courseEntity.Price,
                            lessons = courseEntity.Lessons,
                            capacity = courseEntity.Capacity,
                            startDate = courseEntity.StartDate,
                            endDate = courseEntity.EndDate,
                            Room = new { room.Id, room.Name },
                            Location = new {room.Location.Id, room.Location.Name },
                            Teacher = courseEntity.Teacher == null ? null : courseEntity.Teacher.Select(z => new
                            {
                                z.Id,
                                z.FirstName,
                                z.LastName
                            }),
                            courseSchedule = courseSchedules.Select(x => new
                            {
                                x.Id,
                                x.DayOfWeek,
                                x.StartTime,
                                x.EndTime,
                            })
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                        return BadRequest(ex.Message);
                    }
            
                }

        // PUT: api/Course/5
        [Authorize(Policy = "putCourse")]        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, MapCourseSchedule courseData )
        {
            try
            {
                var Teacher = (await _teacherRepository.GetRelation<ApplicationUser>(n => courseData.TeacherId.Contains(n.Id))).ToList();


                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var room = (await _roomRepo.GetRelation<Room>(x => x.Id == courseData.course.RoomId))
                                    .Select(x => new { x.Id, x.Name, Location = new { x.Location.Id, x.Location.Name } })
                                    .SingleOrDefault();
                if (room == null)
                {
                    return BadRequest("The RoomId does not exist");
                }
                var courseEntity = (await _courseRepository.GetRelation<Course>(n => n.Id == id))
                  .Include(e => e.Teacher).First();

                if (courseEntity == null)
                {
                    return NotFound();
                }
                courseEntity.Teacher.Clear();
                courseEntity.Teacher = Teacher;
                _mapper.Map(courseData.course, courseEntity);
                await _courseRepository.Update(courseEntity);


                await _courseScheduleRepository.DeleteRange(n => n.CourseID == courseEntity.Id);
                List<CourseSchedule> courseSchedules = new List<CourseSchedule>();
                foreach (var courseSched in courseData.courseSchedule)
                {
                    var courseSchedule = new CourseSchedule
                    {
                        DayOfWeek = courseSched.DayOfWeek,
                        StartTime = TimeSpan.Parse(courseSched.StartTime),
                        EndTime = TimeSpan.Parse(courseSched.EndTime),
                        CourseID = courseEntity.Id
                    };
                    courseSchedules.Add(courseSchedule);
                }

                await _courseScheduleRepository.ManyAdd(courseSchedules);

                return Ok(new
                {
                    id = courseEntity.Id,
                    name = courseEntity.Name,
                    price = courseEntity.Price,
                    lessons = courseEntity.Lessons,
                    capacity = courseEntity.Capacity,
                    startDate = courseEntity.StartDate,
                    endDate = courseEntity.EndDate,
                    Room = new { room.Id, room.Name },
                    Location = new { room.Location.Id, room.Location.Name },
                    Teacher = courseEntity.Teacher == null ? null : courseEntity.Teacher.Select(z => new
                    {
                        z.Id,
                        z.FirstName,
                        z.LastName
                    }),
                    courseSchedule = courseSchedules.Select(x => new
                    {
                        x.Id,
                        x.DayOfWeek,
                        x.StartTime,
                        x.EndTime,
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
            
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
                await _courseScheduleRepository.DeleteRange(n => n.CourseID == id);
                await _courseRepository.Delete(id);
                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageStudentsToCourses")]
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
                var students = await _studentRepository.GetRelation<Student>(s => studentIds.Contains(s.Id));

                if (!students.Any())
                {
                    return NotFound("No students found with the provided IDs");
                }

                await students.ForEachAsync(student => student.CourseID = courseID);
                await _studentRepository.ManyUpdate(students);
                return Ok(students.Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.Course.Id,
                    x.Course.Name
                }));

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }



    }
}
