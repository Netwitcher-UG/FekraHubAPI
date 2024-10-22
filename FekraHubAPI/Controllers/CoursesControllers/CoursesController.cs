using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml.Drawing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using static FekraHubAPI.Controllers.CoursesControllers.CoursesController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace FekraHubAPI.Controllers.CoursesControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<CourseSchedule> _courseScheduleRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<ApplicationUser> _teacherRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Room> _roomRepo;
        private readonly ILogger<CoursesController> _logger;
        private readonly IServiceProvider _serviceProvider;
        public CoursesController(IRepository<Course> courseRepository, IRepository<Event> eventRepository,
        IRepository<ApplicationUser> teacherRepository,
            IRepository<Student> studentRepository, IMapper mapper, IRepository<Room> roomRepo,
            ILogger<CoursesController> logger, IRepository<CourseSchedule> courseScheduleRepository, IServiceProvider serviceProvider)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _mapper = mapper;
            _roomRepo = roomRepo;
            _logger = logger;
            _courseScheduleRepository = courseScheduleRepository;
            _serviceProvider = serviceProvider;
            _eventRepository = eventRepository;
        }
        [Authorize]
        [HttpGet("GetCoursesName")]
        public async Task<IActionResult> GetCoursesName(bool? IsAttendance = false)
        {
            try
            {
                var userId = _courseRepository.GetUserIDFromToken(User);
                var isTeacher = await _courseRepository.IsTeacherIDExists(userId);

                var courses = await _courseRepository.GetRelationList(
                    manyWhere: new List<Expression<Func<Course, bool>>?>
                    {
                isTeacher ? (Expression<Func<Course, bool>>)(z => z.Teacher.Any(n => n.Id == userId)) : null,
                IsAttendance == true ? (Expression<Func<Course, bool>>)(x => x.StartDate.Date <= DateTime.Now.Date && x.EndDate.Date >= DateTime.Now.Date) : null,
                (Expression<Func<Course, bool>>)(z => z.Student.Any())
                    }.Where(x => x != null).Cast<Expression<Func<Course, bool>>>().ToList(),

                    selector: x => new { x.Id, x.Name },
                    asNoTracking: true,
                    orderBy: x => x.Id
                );

                if (!courses.Any())
                {
                    return BadRequest("Kein Kurs gefunden."); // No course found
                }

                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }




        }
        public class CourseScheduleCalender
        {
            public int Id { get; set; }
            public string DayOfWeek { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string CourseName { get; set; }
            public int CourseID { get; set; }
        }

        [Authorize(Policy = "GetCourse")]
        [HttpGet("CourseForCalender")]
        public async Task<IActionResult> GetCourseForCalender(int? courseId, DateTime? date)
        {
            try
            {
                var courseSchedule = await _courseScheduleRepository.GetRelationList(
                    where: x => !courseId.HasValue || x.CourseID == courseId.Value,
                    include: x => x.Include(z => z.Course),
                    selector: x => new
                    {
                        CourseId = x.Course.Id,
                        courseName = x.Course.Name,
                        StartDate = x.Course.StartDate.Date,
                        EndDate = x.Course.EndDate.Date,
                        x.StartTime,
                        x.EndTime,
                        x.Id,
                        x.DayOfWeek
                    },
                    asNoTracking: true,
                    orderBy: x => x.Id
                );

                if (!courseSchedule.Any())
                {
                    return BadRequest("Die Kurse wurden nicht gefunden."); // Course not found
                }

                var filteredCourseSchedules = new List<object>();

                // تحديد حدود الفترة المطلوبة: 1 سبتمبر إلى 1 يونيو من السنة التالية
                var startFilter = new DateTime(DateTime.Now.Year, 9, 1); // 1 سبتمبر للسنة الحالية
                var endFilter = new DateTime(DateTime.Now.Year + 1, 6, 1); // 1 يونيو للسنة التالية

                foreach (var schedule in courseSchedule)
                {
                    if (!date.HasValue)
                    {
                        var daysInRange = Enumerable.Range(0, (schedule.EndDate - schedule.StartDate).Days + 1)
                                                    .Select(x => schedule.StartDate.AddDays(x))
                                                    .Where(d => d.DayOfWeek.ToString() == schedule.DayOfWeek &&
                                                                d >= startFilter && d < endFilter) // فلترة التواريخ ضمن الفترة المحددة
                                                    .ToList();

                        foreach (var day in daysInRange)
                        {
                            var startDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.StartTime.Hours, schedule.StartTime.Minutes, 0);
                            var endDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.EndTime.Hours, schedule.EndTime.Minutes, 0);
                            var courseScheduleList = new List<object>
                            {
                                new
                                {
                                    id = schedule.Id,
                                    dayOfWeek = schedule.DayOfWeek,
                                    startTime = startDateTime.ToString("HH:mm:ss"),
                                    endTime = endDateTime.ToString("HH:mm:ss"),
                                    courseName = schedule.courseName,
                                    courseID = schedule.CourseId
                                }
                            };
                            filteredCourseSchedules.Add(new
                            {
                                id = schedule.CourseId,
                                eventName = schedule.courseName,
                                description = "",
                                startDate = startDateTime.Date,
                                endDate = endDateTime.Date,
                                startTime = startDateTime.ToString("HH:mm:ss"),
                                endTime = endDateTime.ToString("HH:mm:ss"),
                                eventType = new
                                {
                                    id = 0,
                                    typeTitle = ""
                                },
                                courseSchedule = courseScheduleList


                            });
                        }
                    }
                    else
                    {
                        var startOfMonth = new DateTime(date.Value.Year, date.Value.Month, 1);
                        var daysInMonth = Enumerable.Range(0, DateTime.DaysInMonth(date.Value.Year, date.Value.Month))
                                                    .Select(day => startOfMonth.AddDays(day))
                                                    .Where(d => d.DayOfWeek.ToString() == schedule.DayOfWeek &&
                                                                d >= startFilter && d < endFilter &&
                                                                d >= schedule.StartDate && d <= schedule.EndDate) // فلترة التواريخ ضمن الفترة المحددة
                                                    .ToList();

                        foreach (var day in daysInMonth)
                        {
                            var startDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.StartTime.Hours, schedule.StartTime.Minutes, 0);
                            var endDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.EndTime.Hours, schedule.EndTime.Minutes, 0);

                            var courseScheduleList = new List<object>
                            {
                                new
                                {
                                    id = schedule.Id,
                                    dayOfWeek = schedule.DayOfWeek,
                                    startTime = startDateTime.ToString("HH:mm:ss"),
                                    endTime = endDateTime.ToString("HH:mm:ss"),
                                    courseName = schedule.courseName,
                                    courseID = schedule.CourseId
                                }
                            };
                            filteredCourseSchedules.Add(new
                            {
                                id = schedule.CourseId,
                                eventName = schedule.courseName,
                                description = "",
                                startDate = startDateTime.Date,
                                endDate = endDateTime.Date,
                                startTime = startDateTime.ToString("HH:mm:ss"),
                                endTime = endDateTime.ToString("HH:mm:ss"),
                                eventType = new
                                {
                                    id = 0,
                                    typeTitle = ""
                                },
                                courseSchedule = courseScheduleList


                            });
                        }
                    }
                }

                if (!filteredCourseSchedules.Any())
                {
                    return BadRequest("Es wurden keine Kurspläne gefunden."); // No course schedules found
                }

                return Ok(filteredCourseSchedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "GetCourse")]
        [HttpGet("CourseEventForCalender")]
        public async Task<IActionResult> GetCombinedEventsAndCourses2([FromQuery]List<int>? courseId, [FromQuery] DateTime? date)
        {
            try
            {
                var CourseWorkingDay = courseId == null || courseId.Count == 0 ? null : await _courseScheduleRepository.GetRelationList(
                         where: x => courseId.Contains(x.Course.Id),
                         selector: x => x.Id,
                         asNoTracking: true);
                var eventE = await _eventRepository.GetRelationList(
                    
                    manyWhere: new List<Expression<Func<Event, bool>>?>
                        {
                        courseId == null || courseId.Count == 0 ? null : (Expression<Func<Event, bool>>)(x => x.CourseSchedule.Any(z => CourseWorkingDay.Contains(z.Id))),
                        date != null ? (Expression<Func<Event, bool>>)(ta => ta.StartDate.Date <= date.Value.Date && ta.EndDate >= date.Value.Date ) : null
                        
                    }.Where(x => x != null).Cast<Expression<Func<Event, bool>>>().ToList(),
                    include: x => x.Include(z => z.EventType).Include(c => c.CourseSchedule),
                selector: x => new
                {
                    x.Id,
                    x.EventName,
                    x.Description,
                    x.StartDate,
                    x.EndDate,
                    x.StartTime,
                    x.EndTime,
                    EventType = x.EventType == null ? null : new
                    {
                        x.EventType.Id,
                        x.EventType.TypeTitle
                    },
                    CourseSchedule = x.CourseSchedule.Select(z => new
                    {
                        z.Id,
                        z.DayOfWeek,
                        z.StartTime,
                        z.EndTime,
                        courseName = z.Course.Name,
                        courseID = z.Course.Id

                    })


                },
                asNoTracking: true);









                var courseSchedule = await _courseScheduleRepository.GetRelationList(
                   where: x => courseId.Count == 0 || courseId.Contains(x.CourseID ?? 0),
                    manyWhere: new List<Expression<Func<CourseSchedule, bool>>?>
                        {
                        courseId == null || courseId.Count == 0 ? null : (Expression<Func<CourseSchedule, bool>>)(x => courseId.Count == 0 || courseId.Contains(x.CourseID ?? 0)),
                        date != null ? (Expression<Func<CourseSchedule, bool>>)(ta => ta.Course.StartDate.Date <= date.Value.Date && ta.Course.EndDate >= date.Value.Date ) : null

                    }.Where(x => x != null).Cast<Expression<Func<CourseSchedule, bool>>>().ToList(),
                   include: x => x.Include(z => z.Course),
                   selector: x => new
                   {
                       CourseId = x.Course.Id,
                       courseName = x.Course.Name,
                       StartDate = x.Course.StartDate.Date,
                       EndDate = x.Course.EndDate.Date,
                       x.StartTime,
                       x.EndTime,
                       x.Id,
                       x.DayOfWeek
                   },
                   asNoTracking: true,
                   orderBy: x => x.Id
               );

                //if (!courseSchedule.Any())
                //{
                //    return BadRequest("Die Kurse wurden nicht gefunden."); // Course not found
                //}

                var filteredCourseSchedules = new List<object>();

                // تحديد حدود الفترة المطلوبة: 1 سبتمبر إلى 1 يونيو من السنة التالية
                var startFilter = new DateTime(DateTime.Now.Year, 9, 1); // 1 سبتمبر للسنة الحالية
                var endFilter = new DateTime(DateTime.Now.Year + 1, 6, 1); // 1 يونيو للسنة التالية

                foreach (var schedule in courseSchedule)
                {
                    if (!date.HasValue)
                    {
                        var daysInRange = Enumerable.Range(0, (schedule.EndDate - schedule.StartDate).Days + 1)
                                                    .Select(x => schedule.StartDate.AddDays(x))
                                                    .Where(d => d.DayOfWeek.ToString() == schedule.DayOfWeek &&
                                                                d >= startFilter && d < endFilter) // فلترة التواريخ ضمن الفترة المحددة
                                                    .ToList();

                        foreach (var day in daysInRange)
                        {
                            var startDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.StartTime.Hours, schedule.StartTime.Minutes, 0);
                            var endDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.EndTime.Hours, schedule.EndTime.Minutes, 0);
                            var courseScheduleList = new List<object>
                            {
                                new
                                {
                                    id = schedule.Id,
                                    dayOfWeek = schedule.DayOfWeek,
                                    startTime = startDateTime.ToString("HH:mm:ss"),
                                    endTime = endDateTime.ToString("HH:mm:ss"),
                                    courseName = schedule.courseName,
                                    courseID = schedule.CourseId
                                }
                            };
                            filteredCourseSchedules.Add(new
                            {
                                id = schedule.CourseId,
                                eventName = schedule.courseName,
                                description = "",
                                startDate = startDateTime.Date,
                                endDate = endDateTime.Date,
                                startTime = startDateTime.ToString("HH:mm:ss"),
                                endTime = endDateTime.ToString("HH:mm:ss"),
                                eventType = new
                                {
                                    id = 0,
                                    typeTitle = ""
                                },
                                courseSchedule = courseScheduleList


                            });
                        }
                    }
                    else
                    {
                        var startOfMonth = new DateTime(date.Value.Year, date.Value.Month, 1);
                        var daysInMonth = Enumerable.Range(0, DateTime.DaysInMonth(date.Value.Year, date.Value.Month))
                                                    .Select(day => startOfMonth.AddDays(day))
                                                    .Where(d => d.DayOfWeek.ToString() == schedule.DayOfWeek &&
                                                                d >= startFilter && d < endFilter &&
                                                                d >= schedule.StartDate && d <= schedule.EndDate) // فلترة التواريخ ضمن الفترة المحددة
                                                    .ToList();

                        foreach (var day in daysInMonth)
                        {
                            var startDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.StartTime.Hours, schedule.StartTime.Minutes, 0);
                            var endDateTime = new DateTime(day.Year, day.Month, day.Day, schedule.EndTime.Hours, schedule.EndTime.Minutes, 0);

                            var courseScheduleList = new List<object>
                            {
                                new
                                {
                                    id = schedule.Id,
                                    dayOfWeek = schedule.DayOfWeek,
                                    startTime = startDateTime.ToString("HH:mm:ss"),
                                    endTime = endDateTime.ToString("HH:mm:ss"),
                                    courseName = schedule.courseName,
                                    courseID = schedule.CourseId
                                }
                            };
                            filteredCourseSchedules.Add(new
                            {
                                id = schedule.CourseId,
                                eventName = schedule.courseName,
                                description = "",
                                startDate = startDateTime.Date,
                                endDate = endDateTime.Date,
                                startTime = startDateTime.ToString("HH:mm:ss"),
                                endTime = endDateTime.ToString("HH:mm:ss"),
                                eventType = new
                                {
                                    id = 0,
                                    typeTitle = ""
                                },
                                courseSchedule = courseScheduleList


                            });
                        }
                    }
                }

                //if (!filteredCourseSchedules.Any())
                //{
                //    return BadRequest("Es wurden keine Kurspläne gefunden."); // No course schedules found
                //}
                List<object> x = new List<object>();
                x.AddRange(eventE);
                x.AddRange(filteredCourseSchedules);

                return Ok(x);


            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventsAndCoursesController", ex.Message));
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
                var isTeacher = await _courseRepository.IsTeacherIDExists(userId);

                var coursesQuery = _courseRepository.GetRelationList(
                    manyWhere: new List<Expression<Func<Course, bool>>?>
                    {
                search != null ? (Expression<Func<Course, bool>>)(x => x.Name.Contains(search)) : null,
                isTeacher ? (Expression<Func<Course, bool>>)(z => z.Teacher.Any(n => n.Id == userId)) : null,
                    }.Where(x => x != null).Cast<Expression<Func<Course, bool>>>().ToList(),
                    include: x => x.Include(r => r.Room).ThenInclude(l => l.Location),
                    selector: sa => new
                    {
                        id = sa.Id,
                        name = sa.Name,
                        price = sa.Price,
                        lessons = sa.Lessons,
                        capacity = sa.Capacity,
                        startDate = sa.StartDate,
                        endDate = sa.EndDate,
                        Room = sa.Room == null ? null : new { sa.Room.Id, sa.Room.Name },
                        Location = sa.Room == null || sa.Room.Location == null ? null : new { sa.Room.Location.Id, sa.Room.Location.Name }
                    },
                    asNoTracking: true,
                    orderBy: x => x.Id
                );

                var courses = await coursesQuery;
                if (!courses.Any()) return Ok(Enumerable.Empty<object>());

                var courseIds = courses.Select(c => c.id).ToList();

                var teachersTask = Task.Run(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        return await context.Courses
                            .Where(x => courseIds.Contains(x.Id))
                            .Include(x => x.Teacher)
                            .Select(sa => new
                            {
                                courseId = sa.Id,
                                Teacher = sa.Teacher == null ? null : sa.Teacher.Select(z => new
                                {
                                    z.Id,
                                    z.FirstName,
                                    z.LastName
                                })
                            })
                            .AsNoTracking()
                            .ToListAsync();
                    }
                });

                var courseSchedulesTask = Task.Run(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        return await context.Courses
                            .Where(x => courseIds.Contains(x.Id))
                            .Include(x => x.CourseSchedule)
                            .Select(sa => new
                            {
                                courseId = sa.Id,
                                CourseSchedule = sa.CourseSchedule == null ? null : sa.CourseSchedule.Select(x => new
                                {
                                    x.Id,
                                    x.DayOfWeek,
                                    x.StartTime,
                                    x.EndTime
                                })
                            })
                            .AsNoTracking()
                            .ToListAsync();
                    }
                });

                // انتظار تنفيذ المهام المتوازية
                await Task.WhenAll(teachersTask, courseSchedulesTask).ConfigureAwait(false);

                var teachers = await teachersTask;
                var courseSchedules = await courseSchedulesTask;

                var finalCourses = courses.Select(course => new
                {
                    course.id,
                    course.name,
                    course.price,
                    course.lessons,
                    course.capacity,
                    course.startDate,
                    course.endDate,
                    course.Room,
                    course.Location,

                    Teacher = teachers.Where(t => t.courseId == course.id)
                        .SelectMany(t => t.Teacher?.Select(z => new
                        {
                            z.Id,
                            z.FirstName,
                            z.LastName
                        }) ?? Enumerable.Empty<object>())
                        .ToList(),

                    CourseSchedule = courseSchedules.Where(cs => cs.courseId == course.id)
                        .SelectMany(cs => cs.CourseSchedule?.Select(x => new
                        {
                            x.Id,
                            x.DayOfWeek,
                            x.StartTime,
                            x.EndTime
                        }) ?? Enumerable.Empty<object>())
                        .ToList()
                });

                return Ok(finalCourses);

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
                var courses = await _courseRepository.GetRelationSingle(
                    where: x => x.Id == id,
                    manyWhere: new List<Expression<Func<Course, bool>>?>
                    {
                        Teacher ? (Expression<Func<Course, bool>>)(z => z.Teacher.Select(n => n.Id).Contains(userId)) : null,
                    }.Where(x => x != null).Cast<Expression<Func<Course, bool>>>().ToList(),
                    include: x => x.Include(r => r.Room).Include(l => l.Room.Location).Include(t => t.Teacher).Include(c => c.CourseSchedule),
                    selector: sa => new
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
                        }),
                        CourseSchedule = sa.CourseSchedule == null ? null : sa.CourseSchedule.Select(x => new
                        {
                            x.Id,
                            x.DayOfWeek,
                            x.StartTime,
                            x.EndTime
                        }),
                    },
                    returnType: QueryReturnType.FirstOrDefault,
                    asNoTracking: true);

                if (courses == null)
                {
                    return BadRequest("Kurs nicht gefunden.");//Course not found
                }
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }




        }
        public class MapCourseSchedule
        {
            public string[]? TeacherId { get; set; }
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
                if (mapCourseSchedule.course.StartDate.Date >= mapCourseSchedule.course.EndDate.Date)
                {
                    return BadRequest("Ungültiges Datum: Das Startdatum ist größer als das Enddatum.");//Invalid date: Start date is greater than end date
                }
                foreach (var schedule in mapCourseSchedule.courseSchedule)
                {
                    if (TimeSpan.Parse(schedule.StartTime) >= TimeSpan.Parse(schedule.EndTime))
                    {
                        return BadRequest("Die Startzeit muss vor der Endzeit liegen.");//Start time must be before end time.
                    }
                }
                for (int i = 0; i < mapCourseSchedule.courseSchedule.Count; i++)
                {
                    var schedule1 = mapCourseSchedule.courseSchedule[i];
                    for (int j = i + 1; j < mapCourseSchedule.courseSchedule.Count; j++)
                    {
                        var schedule2 = mapCourseSchedule.courseSchedule[j];

                        if (schedule1.DayOfWeek == schedule2.DayOfWeek)
                        {
                            var start1 = TimeSpan.Parse(schedule1.StartTime);
                            var end1 = TimeSpan.Parse(schedule1.EndTime);
                            var start2 = TimeSpan.Parse(schedule2.StartTime);
                            var end2 = TimeSpan.Parse(schedule2.EndTime);

                            if (start1 < end2 && start2 < end1)
                            {
                                return BadRequest($"Die Zeitpläne für den gleichen Tag ({schedule1.DayOfWeek}) überschneiden sich.");//Schedules for the same day ({schedule1.DayOfWeek}) are overlapping.
                            }
                        }
                    }
                }


                var room = await _roomRepo.GetRelationSingle(
                            where: x => x.Id == mapCourseSchedule.course.RoomId,
                            include: x => x.Include(l => l.Location),
                            selector: x => new { x.Id, x.Name, Location = new { x.Location.Id, x.Location.Name } },
                            returnType: QueryReturnType.SingleOrDefault,
                            asNoTracking: true);
                if (room == null)
                {
                    return BadRequest("Raum nicht gefunden");//Room not found
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
                if (mapCourseSchedule.TeacherId != null)
                {
                    var teachers = await _teacherRepository.GetRelationList(
                        where: n => mapCourseSchedule.TeacherId.Contains(n.Id),
                        selector: x => x);
                    courseEntity.Teacher = teachers;
                }

                await _courseRepository.Add(courseEntity);

                List<CourseSchedule> courseSchedules = new List<CourseSchedule>();
                foreach (var courseSched in mapCourseSchedule.courseSchedule)
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

                return Ok("Erfolgreich hinzugefügt.");//added success
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
        public async Task<IActionResult> PutCourse(int id, MapCourseSchedule courseData)
        {
            try
            {
                var Teacher = new List<ApplicationUser>();
                if (courseData.TeacherId != null)
                {
                    Teacher = await _teacherRepository.GetRelationList(
                    where: n => courseData.TeacherId.Contains(n.Id),
                    selector: x => x
                    );
                }



                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (courseData.course.StartDate.Date >= courseData.course.EndDate.Date)
                {
                    return BadRequest("Ungültiges Datum: Das Startdatum ist größer als das Enddatum.");//Invalid date: Start date is greater than end date
                }
                foreach (var schedule in courseData.courseSchedule)
                {
                    if (TimeSpan.Parse(schedule.StartTime) >= TimeSpan.Parse(schedule.EndTime))
                    {
                        return BadRequest("Die Startzeit muss vor der Endzeit liegen.");//Start time must be before end time.
                    }
                }
                for (int i = 0; i < courseData.courseSchedule.Count; i++)
                {
                    var schedule1 = courseData.courseSchedule[i];
                    for (int j = i + 1; j < courseData.courseSchedule.Count; j++)
                    {
                        var schedule2 = courseData.courseSchedule[j];

                        if (schedule1.DayOfWeek == schedule2.DayOfWeek)
                        {
                            var start1 = TimeSpan.Parse(schedule1.StartTime);
                            var end1 = TimeSpan.Parse(schedule1.EndTime);
                            var start2 = TimeSpan.Parse(schedule2.StartTime);
                            var end2 = TimeSpan.Parse(schedule2.EndTime);

                            if (start1 < end2 && start2 < end1)
                            {
                                return BadRequest($"Die Zeitpläne für den gleichen Tag ({schedule1.DayOfWeek}) überschneiden sich.");//Schedules for the same day ({schedule1.DayOfWeek}) are overlapping.
                            }
                        }
                    }
                }
                var room = await _roomRepo.GetRelationSingle(
                    where: x => x.Id == courseData.course.RoomId,
                    include: x => x.Include(l => l.Location),
                    selector: x => new { x.Id, x.Name, Location = new { x.Location.Id, x.Location.Name } },
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);

                if (room == null)
                {
                    return BadRequest("Raum nicht gefunden");//Room not found
                }
                var courseEntity = await _courseRepository.GetRelationSingle(
                    where: n => n.Id == id,
                    include: x => x.Include(e => e.Teacher),
                    selector: x => x,
                    returnType: QueryReturnType.FirstOrDefault);

                if (courseEntity == null)
                {
                    return BadRequest("Kurs nicht gefunden.");//Course not found
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
                var courseEntity = await _courseRepository.DataExist(x => x.Id == id);
                if (!courseEntity)
                {
                    return BadRequest("Kurs nicht gefunden.");//Course not found
                }
                var studentExist = await _studentRepository.DataExist(n => n.CourseID == id);
                if (studentExist)
                {
                    return BadRequest("Dieser Kurs enthält Schüler!!");//This course contains students !!
                }
                await _courseScheduleRepository.DeleteRange(n => n.CourseID == id);
                await _courseRepository.Delete(id);
                return Ok("Erfolgreich gelöscht");//Deleted success
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "CoursesController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageStudentsToCourses")]
        [HttpPost("AssignStudentsToCourse")]
        public async Task<IActionResult> AssignStudentsToCourse(int courseID, [FromBody] List<int> studentIds)
        {
            try
            {
                if (courseID <= 0 || studentIds == null || !studentIds.Any())
                {
                    return BadRequest("Ungültige Kurs-ID oder Schülerliste.");//Invalid course ID or student list
                }

                var course = await _courseRepository.GetById(courseID);
                if (course == null)
                {
                    return BadRequest("Kurs nicht gefunden.");//Course not found
                }
                var students = await _studentRepository.GetRelationList(
                    where: s => studentIds.Contains(s.Id),
                    selector: x => x);

                if (!students.Any())
                {
                    return BadRequest("Keine Schüler mit den angegebenen IDs gefunden.");//No students found with the provided IDs
                }

                foreach (var student in students)
                {
                    student.CourseID = courseID;
                }
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
