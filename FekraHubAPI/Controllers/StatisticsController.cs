using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FekraHubAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "GetStatistics")]
    public class StatisticsController : ControllerBase
    {
        private const string AdminRoleId = "1";
        private const string SecretariatRoleId = "2";
        private const string ParentRoleId = "3";
        private const string TeacherRoleId = "4";

        private readonly ApplicationDbContext _context;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            ApplicationDbContext context,
            ILogger<StatisticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            var cancellationToken = HttpContext.RequestAborted;

            try
            {
                var now = DateTime.UtcNow;
                var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                var tomorrow = today.AddDays(1);
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var nextMonthStart = monthStart.AddMonths(1);
                var chartStart = monthStart.AddMonths(-11);
                var next30Days = now.AddDays(30);

                // =========================================================
                // Users
                // =========================================================
                var users = await _context.ApplicationUser
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Active = g.Count(x => x.ActiveUser),
                        Inactive = g.Count(x => !x.ActiveUser),
                        EmailConfirmed = g.Count(x => x.EmailConfirmed),
                        EmailNotConfirmed = g.Count(x => !x.EmailConfirmed),
                        RegisteredThisMonth = g.Count(x =>
                            x.RegistrationDate >= monthStart &&
                            x.RegistrationDate < nextMonthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var roleCounts = await _context.UserRoles
                    .AsNoTracking()
                    .GroupBy(x => x.RoleId)
                    .Select(g => new
                    {
                        RoleId = g.Key,
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(x => x.RoleId, x => x.Count, cancellationToken);

                // =========================================================
                // Students / Admissions
                // =========================================================
                var students = await _context.Students
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Active = g.Count(x => x.ActiveStudent),
                        Inactive = g.Count(x => !x.ActiveStudent),
                        AdminApproved = g.Count(x => x.AdminApproved == true),
                        ParentApproved = g.Count(x => x.ParentApproved == true),
                        FullyApproved = g.Count(x =>
                            x.AdminApproved == true &&
                            x.ParentApproved == true),
                        PendingAdminApproval = g.Count(x =>
                            !x.ActiveStudent &&
                            x.ParentApproved == true &&
                            x.AdminApproved != true),
                        AwaitingParentApproval = g.Count(x =>
                            !x.ActiveStudent &&
                            x.AdminApproved == true &&
                            x.ParentApproved != true),
                        AssignedToCourse = g.Count(x => x.CourseID != null),
                        ActiveAssignedToCourse = g.Count(x =>
                            x.ActiveStudent && x.CourseID != null),
                        AddedThisMonth = g.Count(x =>
                            x.CreatedAt.HasValue &&
                            x.CreatedAt.Value >= monthStart &&
                            x.CreatedAt.Value < nextMonthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                // =========================================================
                // Courses
                // =========================================================
                var courses = await _context.Courses
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Active = g.Count(x => x.StartDate <= now && x.EndDate >= now),
                        Upcoming = g.Count(x => x.StartDate > now),
                        Finished = g.Count(x => x.EndDate < now),
                        TotalCapacity = g.Sum(x => x.Capacity),
                        TotalLessons = g.Sum(x => x.Lessons)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var studentsPerCourse = await _context.Courses
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new CourseUsageDto
                    {
                        CourseId = x.Id,
                        CourseName = x.Name,
                        Capacity = x.Capacity,
                        AssignedStudents = x.Student.Count(),
                        ActiveStudents = x.Student.Count(s => s.ActiveStudent)
                    })
                    .ToListAsync(cancellationToken);

                var totalCapacity = courses?.TotalCapacity ?? 0;
                var assignedStudents = students?.AssignedToCourse ?? 0;
                var availableSeats = Math.Max(0, totalCapacity - assignedStudents);

                foreach (var course in studentsPerCourse)
                {
                    course.AvailableSeats = Math.Max(0, course.Capacity - course.AssignedStudents);
                    course.OccupancyPercentage = Percentage(course.AssignedStudents, course.Capacity);
                    course.AvailableSeatsPercentage = Percentage(course.AvailableSeats, course.Capacity);
                    course.ActiveStudentsPercentage = Percentage(course.ActiveStudents, course.AssignedStudents);
                    course.StudentDistributionPercentage = Percentage(course.AssignedStudents, assignedStudents);
                }

                // =========================================================
                // Attendance statuses
                // =========================================================
                var attendanceStatuses = await _context.AttendanceStatuses
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.Id,
                        x.Title
                    })
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => x.Title ?? string.Empty,
                        cancellationToken);

                // =========================================================
                // Student attendance - today
                // =========================================================
                var studentAttendanceTodayBuckets = await _context.StudentAttendances
                    .AsNoTracking()
                    .Where(x => x.date >= today && x.date < tomorrow)
                    .GroupBy(x => x.StatusID)
                    .Select(g => new AttendanceBucketDto
                    {
                        StatusId = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                // Last 12 months also provides current-month statistics.
                var studentAttendanceMonthlyBuckets = await _context.StudentAttendances
                    .AsNoTracking()
                    .Where(x => x.date >= chartStart && x.date < nextMonthStart)
                    .GroupBy(x => new
                    {
                        Year = x.date.Year,
                        Month = x.date.Month,
                        x.StatusID
                    })
                    .Select(g => new AttendanceMonthBucketDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        StatusId = g.Key.StatusID,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                // =========================================================
                // Teacher attendance - today / last 12 months
                // =========================================================
                var teacherAttendanceTodayBuckets = await _context.TeacherAttendances
                    .AsNoTracking()
                    .Where(x => x.date >= today && x.date < tomorrow)
                    .GroupBy(x => x.StatusID)
                    .Select(g => new AttendanceBucketDto
                    {
                        StatusId = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                var teacherAttendanceMonthlyBuckets = await _context.TeacherAttendances
                    .AsNoTracking()
                    .Where(x => x.date >= chartStart && x.date < nextMonthStart)
                    .GroupBy(x => new
                    {
                        Year = x.date.Year,
                        Month = x.date.Month,
                        x.StatusID
                    })
                    .Select(g => new AttendanceMonthBucketDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        StatusId = g.Key.StatusID,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                var studentAttendanceToday = BuildAttendanceSummary(
                    studentAttendanceTodayBuckets,
                    attendanceStatuses);

                var studentAttendanceThisMonth = BuildAttendanceSummary(
                    studentAttendanceMonthlyBuckets
                        .Where(x => x.Year == now.Year && x.Month == now.Month)
                        .Select(x => new AttendanceBucketDto
                        {
                            StatusId = x.StatusId,
                            Count = x.Count
                        }),
                    attendanceStatuses);

                var teacherAttendanceToday = BuildAttendanceSummary(
                    teacherAttendanceTodayBuckets,
                    attendanceStatuses);

                var teacherAttendanceThisMonth = BuildAttendanceSummary(
                    teacherAttendanceMonthlyBuckets
                        .Where(x => x.Year == now.Year && x.Month == now.Month)
                        .Select(x => new AttendanceBucketDto
                        {
                            StatusId = x.StatusId,
                            Count = x.Count
                        }),
                    attendanceStatuses);

                // =========================================================
                // Reports
                // Improved is the current nullable report status in this project.
                // true  = accepted, false = not accepted, null = pending review.
                // =========================================================
                var reports = await _context.Reports
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Accepted = g.Count(x => x.Improved == true),
                        NotAccepted = g.Count(x => x.Improved == false),
                        Pending = g.Count(x => x.Improved == null),
                        ThisMonth = g.Count(x =>
                            x.CreationDate >= monthStart &&
                            x.CreationDate < nextMonthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                // =========================================================
                // Events
                // =========================================================
                var events = await _context.Events
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Ongoing = g.Count(x => x.StartDate <= now && x.EndDate >= now),
                        Upcoming = g.Count(x => x.StartDate > now),
                        UpcomingNext30Days = g.Count(x =>
                            x.StartDate > now && x.StartDate <= next30Days),
                        Finished = g.Count(x => x.EndDate < now),
                        ThisMonth = g.Count(x =>
                            x.StartDate < nextMonthStart &&
                            x.EndDate >= monthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                // =========================================================
                // Contracts / invoices / payroll documents
                // There is no monetary amount field in Invoice/PayRoll models,
                // therefore these are document counts, not financial totals.
                // =========================================================
                var studentContracts = await _context.StudentContract
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        ThisMonth = g.Count(x =>
                            x.CreationDate >= monthStart &&
                            x.CreationDate < nextMonthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var workContracts = await _context.WorkContracts
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        ThisMonth = g.Count(x =>
                            x.Timestamp >= monthStart &&
                            x.Timestamp < nextMonthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var invoices = await _context.Invoices
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        ThisMonth = g.Count(x =>
                            x.Date >= monthStart &&
                            x.Date < nextMonthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var payrolls = await _context.PayRoll
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        ThisMonth = g.Count(x =>
                            x.Timestamp >= monthStart &&
                            x.Timestamp < nextMonthStart)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                // =========================================================
                // Additional useful school resources
                // =========================================================
                var roomsCount = await _context.Rooms.AsNoTracking().CountAsync(cancellationToken);
                var locationsCount = await _context.Location.AsNoTracking().CountAsync(cancellationToken);
                var uploadsCount = await _context.Uploads.AsNoTracking().CountAsync(cancellationToken);

                // =========================================================
                // Charts - last 12 months
                // =========================================================
                var months = BuildLast12Months(monthStart);

                var usersPerMonthRaw = await _context.ApplicationUser
                    .AsNoTracking()
                    .Where(x =>
                        x.RegistrationDate >= chartStart &&
                        x.RegistrationDate < nextMonthStart)
                    .GroupBy(x => new
                    {
                        Year = x.RegistrationDate.Year,
                        Month = x.RegistrationDate.Month
                    })
                    .Select(g => new MonthBucketDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                var studentsPerMonthRaw = await _context.Students
                    .AsNoTracking()
                    .Where(x =>
                        x.CreatedAt.HasValue &&
                        x.CreatedAt.Value >= chartStart &&
                        x.CreatedAt.Value < nextMonthStart)
                    .GroupBy(x => new
                    {
                        Year = x.CreatedAt!.Value.Year,
                        Month = x.CreatedAt!.Value.Month
                    })
                    .Select(g => new MonthBucketDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                var reportsPerMonthRaw = await _context.Reports
                    .AsNoTracking()
                    .Where(x =>
                        x.CreationDate >= chartStart &&
                        x.CreationDate < nextMonthStart)
                    .GroupBy(x => new
                    {
                        Year = x.CreationDate.Year,
                        Month = x.CreationDate.Month
                    })
                    .Select(g => new MonthBucketDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                var invoicesPerMonthRaw = await _context.Invoices
                    .AsNoTracking()
                    .Where(x => x.Date >= chartStart && x.Date < nextMonthStart)
                    .GroupBy(x => new
                    {
                        Year = x.Date.Year,
                        Month = x.Date.Month
                    })
                    .Select(g => new MonthBucketDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                var eventsPerMonthRaw = await _context.Events
                    .AsNoTracking()
                    .Where(x => x.StartDate >= chartStart && x.StartDate < nextMonthStart)
                    .GroupBy(x => new
                    {
                        Year = x.StartDate.Year,
                        Month = x.StartDate.Month
                    })
                    .Select(g => new MonthBucketDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                var response = new
                {
                    generatedAtUtc = now,

                    users = new
                    {
                        total = users?.Total ?? 0,
                        active = users?.Active ?? 0,
                        activePercentage = Percentage(users?.Active ?? 0, users?.Total ?? 0),
                        inactive = users?.Inactive ?? 0,
                        inactivePercentage = Percentage(users?.Inactive ?? 0, users?.Total ?? 0),
                        emailConfirmed = users?.EmailConfirmed ?? 0,
                        emailConfirmedPercentage = Percentage(users?.EmailConfirmed ?? 0, users?.Total ?? 0),
                        emailNotConfirmed = users?.EmailNotConfirmed ?? 0,
                        emailNotConfirmedPercentage = Percentage(users?.EmailNotConfirmed ?? 0, users?.Total ?? 0),
                        registeredThisMonth = users?.RegisteredThisMonth ?? 0,
                        admins = GetRoleCount(roleCounts, AdminRoleId),
                        secretaries = GetRoleCount(roleCounts, SecretariatRoleId),
                        parents = GetRoleCount(roleCounts, ParentRoleId),
                        teachers = GetRoleCount(roleCounts, TeacherRoleId)
                    },

                    students = new
                    {
                        total = students?.Total ?? 0,
                        active = students?.Active ?? 0,
                        activePercentage = Percentage(students?.Active ?? 0, students?.Total ?? 0),
                        inactive = students?.Inactive ?? 0,
                        inactivePercentage = Percentage(students?.Inactive ?? 0, students?.Total ?? 0),
                        adminApproved = students?.AdminApproved ?? 0,
                        parentApproved = students?.ParentApproved ?? 0,
                        fullyApproved = students?.FullyApproved ?? 0,
                        fullyApprovedPercentage = Percentage(students?.FullyApproved ?? 0, students?.Total ?? 0),
                        assignedToCourse = students?.AssignedToCourse ?? 0,
                        assignedToCoursePercentage = Percentage(students?.AssignedToCourse ?? 0, students?.Total ?? 0),
                        unassignedToCourse = Math.Max(0, (students?.Total ?? 0) - (students?.AssignedToCourse ?? 0)),
                        unassignedToCoursePercentage = Percentage(
                            Math.Max(0, (students?.Total ?? 0) - (students?.AssignedToCourse ?? 0)),
                            students?.Total ?? 0),
                        activeAssignedToCourse = students?.ActiveAssignedToCourse ?? 0,
                        addedThisMonth = students?.AddedThisMonth ?? 0
                    },

                    admissions = new
                    {
                        totalCurrentStudents = students?.Total ?? 0,
                        pendingAdminApproval = students?.PendingAdminApproval ?? 0,
                        pendingAdminApprovalPercentage = Percentage(students?.PendingAdminApproval ?? 0, students?.Total ?? 0),
                        awaitingParentApproval = students?.AwaitingParentApproval ?? 0,
                        awaitingParentApprovalPercentage = Percentage(students?.AwaitingParentApproval ?? 0, students?.Total ?? 0),
                        approvedByAdmin = students?.AdminApproved ?? 0,
                        approvedByAdminPercentage = Percentage(students?.AdminApproved ?? 0, students?.Total ?? 0),
                        fullyApproved = students?.FullyApproved ?? 0,
                        fullyApprovedPercentage = Percentage(students?.FullyApproved ?? 0, students?.Total ?? 0)
                    },

                    courses = new
                    {
                        total = courses?.Total ?? 0,
                        active = courses?.Active ?? 0,
                        activePercentage = Percentage(courses?.Active ?? 0, courses?.Total ?? 0),
                        upcoming = courses?.Upcoming ?? 0,
                        upcomingPercentage = Percentage(courses?.Upcoming ?? 0, courses?.Total ?? 0),
                        finished = courses?.Finished ?? 0,
                        finishedPercentage = Percentage(courses?.Finished ?? 0, courses?.Total ?? 0),
                        totalLessons = courses?.TotalLessons ?? 0,
                        totalCapacity,
                        assignedStudents,
                        activeAssignedStudents = students?.ActiveAssignedToCourse ?? 0,
                        activeAssignedStudentsPercentage = Percentage(students?.ActiveAssignedToCourse ?? 0, assignedStudents),
                        availableSeats,
                        occupancyPercentage = Percentage(assignedStudents, totalCapacity),
                        availableSeatsPercentage = Percentage(availableSeats, totalCapacity)
                    },

                    attendance = new
                    {
                        students = new
                        {
                            today = studentAttendanceToday,
                            thisMonth = studentAttendanceThisMonth
                        },
                        teachers = new
                        {
                            today = teacherAttendanceToday,
                            thisMonth = teacherAttendanceThisMonth
                        }
                    },

                    reports = new
                    {
                        total = reports?.Total ?? 0,
                        accepted = reports?.Accepted ?? 0,
                        acceptedPercentage = Percentage(reports?.Accepted ?? 0, reports?.Total ?? 0),
                        notAccepted = reports?.NotAccepted ?? 0,
                        notAcceptedPercentage = Percentage(reports?.NotAccepted ?? 0, reports?.Total ?? 0),
                        pending = reports?.Pending ?? 0,
                        pendingPercentage = Percentage(reports?.Pending ?? 0, reports?.Total ?? 0),
                        thisMonth = reports?.ThisMonth ?? 0
                    },

                    events = new
                    {
                        total = events?.Total ?? 0,
                        ongoing = events?.Ongoing ?? 0,
                        ongoingPercentage = Percentage(events?.Ongoing ?? 0, events?.Total ?? 0),
                        upcoming = events?.Upcoming ?? 0,
                        upcomingPercentage = Percentage(events?.Upcoming ?? 0, events?.Total ?? 0),
                        upcomingNext30Days = events?.UpcomingNext30Days ?? 0,
                        finished = events?.Finished ?? 0,
                        finishedPercentage = Percentage(events?.Finished ?? 0, events?.Total ?? 0),
                        thisMonth = events?.ThisMonth ?? 0
                    },

                    contracts = new
                    {
                        studentContracts = new
                        {
                            total = studentContracts?.Total ?? 0,
                            thisMonth = studentContracts?.ThisMonth ?? 0
                        },
                        workContracts = new
                        {
                            total = workContracts?.Total ?? 0,
                            thisMonth = workContracts?.ThisMonth ?? 0
                        }
                    },

                    documents = new
                    {
                        invoices = new
                        {
                            total = invoices?.Total ?? 0,
                            thisMonth = invoices?.ThisMonth ?? 0
                        },
                        payrolls = new
                        {
                            total = payrolls?.Total ?? 0,
                            thisMonth = payrolls?.ThisMonth ?? 0
                        }
                    },

                    resources = new
                    {
                        rooms = roomsCount,
                        locations = locationsCount,
                        uploads = uploadsCount
                    },

                    charts = new
                    {
                        usersRegisteredPerMonth = BuildMonthSeries(months, usersPerMonthRaw),
                        studentsAddedPerMonth = BuildMonthSeries(months, studentsPerMonthRaw),
                        reportsPerMonth = BuildMonthSeries(months, reportsPerMonthRaw),
                        invoicesPerMonth = BuildMonthSeries(months, invoicesPerMonthRaw),
                        eventsPerMonth = BuildMonthSeries(months, eventsPerMonthRaw),
                        studentAttendancePerMonth = BuildAttendanceMonthSeries(
                            months,
                            studentAttendanceMonthlyBuckets,
                            attendanceStatuses),
                        teacherAttendancePerMonth = BuildAttendanceMonthSeries(
                            months,
                            teacherAttendanceMonthlyBuckets,
                            attendanceStatuses),
                        studentsPerCourse
                    }
                };

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build statistics dashboard.");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Failed to load dashboard statistics."
                });
            }
        }

        // =============================================================
        // Helpers
        // =============================================================

        private static int GetRoleCount(
            IReadOnlyDictionary<string, int> roleCounts,
            string roleId)
        {
            return roleCounts.TryGetValue(roleId, out var count) ? count : 0;
        }

        private static double Percentage(int value, int total)
        {
            return total > 0
                ? Math.Round(value * 100d / total, 2)
                : 0;
        }

        private static List<MonthKeyDto> BuildLast12Months(DateTime currentMonthStart)
        {
            var result = new List<MonthKeyDto>(12);

            for (var i = 11; i >= 0; i--)
            {
                var date = currentMonthStart.AddMonths(-i);
                result.Add(new MonthKeyDto
                {
                    Year = date.Year,
                    Month = date.Month,
                    Label = $"{date:yyyy-MM}"
                });
            }

            return result;
        }

        private static List<MonthCountDto> BuildMonthSeries(
            IEnumerable<MonthKeyDto> months,
            IEnumerable<MonthBucketDto> values)
        {
            var lookup = values.ToDictionary(
                x => (x.Year, x.Month),
                x => x.Count);

            return months
                .Select(month => new MonthCountDto
                {
                    Year = month.Year,
                    Month = month.Month,
                    Label = month.Label,
                    Count = lookup.TryGetValue((month.Year, month.Month), out var count)
                        ? count
                        : 0
                })
                .ToList();
        }

        private static AttendanceSummaryDto BuildAttendanceSummary(
            IEnumerable<AttendanceBucketDto> buckets,
            IReadOnlyDictionary<int, string> statuses)
        {
            var byStatus = buckets
                .GroupBy(x => x.StatusId)
                .Select(g =>
                {
                    var statusId = g.Key;
                    var statusTitle = statusId.HasValue &&
                                      statuses.TryGetValue(statusId.Value, out var title)
                        ? title
                        : "Unknown";

                    return new AttendanceStatusCountDto
                    {
                        StatusId = statusId,
                        Status = statusTitle,
                        Count = g.Sum(x => x.Count)
                    };
                })
                .OrderBy(x => x.StatusId)
                .ToList();

            var total = byStatus.Sum(x => x.Count);
            var present = byStatus
                .Where(x => IsPresentStatus(x.Status))
                .Sum(x => x.Count);
            var absent = byStatus
                .Where(x => IsAbsentStatus(x.Status))
                .Sum(x => x.Count);
            var other = Math.Max(0, total - present - absent);

            foreach (var status in byStatus)
            {
                status.Percentage = Percentage(status.Count, total);
            }

            return new AttendanceSummaryDto
            {
                TotalRecords = total,
                Present = present,
                PresentPercentage = Percentage(present, total),
                Absent = absent,
                AbsentPercentage = Percentage(absent, total),
                Other = other,
                OtherPercentage = Percentage(other, total),
                ByStatus = byStatus
            };
        }

        private static List<AttendanceMonthDto> BuildAttendanceMonthSeries(
            IEnumerable<MonthKeyDto> months,
            IReadOnlyCollection<AttendanceMonthBucketDto> buckets,
            IReadOnlyDictionary<int, string> statuses)
        {
            return months
                .Select(month =>
                {
                    var summary = BuildAttendanceSummary(
                        buckets
                            .Where(x => x.Year == month.Year && x.Month == month.Month)
                            .Select(x => new AttendanceBucketDto
                            {
                                StatusId = x.StatusId,
                                Count = x.Count
                            }),
                        statuses);

                    return new AttendanceMonthDto
                    {
                        Year = month.Year,
                        Month = month.Month,
                        Label = month.Label,
                        TotalRecords = summary.TotalRecords,
                        Present = summary.Present,
                        PresentPercentage = summary.PresentPercentage,
                        Absent = summary.Absent,
                        AbsentPercentage = summary.AbsentPercentage,
                        Other = summary.Other,
                        OtherPercentage = summary.OtherPercentage,
                        ByStatus = summary.ByStatus
                    };
                })
                .ToList();
        }

        private static bool IsPresentStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var value = status.Trim().ToLowerInvariant();

            return value == "present" ||
                   value.Contains("anwesend") ||
                   value.Contains("anwesen") ||
                   value.Contains("حاضر") ||
                   value.Contains("حضور");
        }

        private static bool IsAbsentStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var value = status.Trim().ToLowerInvariant();

            return value == "absent" ||
                   value.Contains("abwesend") ||
                   value.Contains("fehlt") ||
                   value.Contains("fehlend") ||
                   value.Contains("غائب") ||
                   value.Contains("غياب");
        }

        // =============================================================
        // Internal DTOs - kept in this controller intentionally.
        // =============================================================

        private sealed class MonthKeyDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string Label { get; set; } = string.Empty;
        }

        private sealed class MonthBucketDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int Count { get; set; }
        }

        private sealed class MonthCountDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string Label { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private sealed class AttendanceBucketDto
        {
            public int? StatusId { get; set; }
            public int Count { get; set; }
        }

        private sealed class AttendanceMonthBucketDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int? StatusId { get; set; }
            public int Count { get; set; }
        }

        private sealed class AttendanceStatusCountDto
        {
            public int? StatusId { get; set; }
            public string Status { get; set; } = string.Empty;
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        private sealed class AttendanceSummaryDto
        {
            public int TotalRecords { get; set; }
            public int Present { get; set; }
            public double PresentPercentage { get; set; }
            public int Absent { get; set; }
            public double AbsentPercentage { get; set; }
            public int Other { get; set; }
            public double OtherPercentage { get; set; }
            public List<AttendanceStatusCountDto> ByStatus { get; set; } = new();
        }

        private sealed class AttendanceMonthDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string Label { get; set; } = string.Empty;
            public int TotalRecords { get; set; }
            public int Present { get; set; }
            public double PresentPercentage { get; set; }
            public int Absent { get; set; }
            public double AbsentPercentage { get; set; }
            public int Other { get; set; }
            public double OtherPercentage { get; set; }
            public List<AttendanceStatusCountDto> ByStatus { get; set; } = new();
        }

        private sealed class CourseUsageDto
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; } = string.Empty;
            public int Capacity { get; set; }
            public int AssignedStudents { get; set; }
            public int ActiveStudents { get; set; }
            public int AvailableSeats { get; set; }
            public double OccupancyPercentage { get; set; }
            public double AvailableSeatsPercentage { get; set; }
            public double ActiveStudentsPercentage { get; set; }
            public double StudentDistributionPercentage { get; set; }
        }
    }
}
