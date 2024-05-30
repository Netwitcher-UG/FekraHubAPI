using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Seeds;

using Microsoft.Extensions.Hosting;

namespace FekraHubAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    //DbContext
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options 
            ): base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
            SeedAdminUser(builder);
        }

        private  static void SeedRoles(ModelBuilder builder)
        {
            DefaultRole.SeedRoleAsync(builder);
            DefaultPermissions.SeedClaimsAsync(builder);
        }

        private static void SeedAdminUser(ModelBuilder builder )
        {
            DefaultUser.SeedAdminAsync(builder);


        }

        public DbSet<AspNetPermissions> AspNetPermissions { get; set; }
        public DbSet<Course> Courses { get; set; }
        

        public DbSet<AttendanceStatus> AttendanceStatuses { get; set; }
        public DbSet<CourseEvent> CourseEvents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<CourseSchedule> CourseSchedules { get; set; }
        public DbSet<EventType> EventsTypes { get; set; }
        public DbSet<StudentContract> StudentContract { get; set; }
        public DbSet<ParentInvoice> ParentInvoices { get; set; }
        public DbSet<PayRoll> PayRoll { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentAttendance> StudentAttendances { get; set; }
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; }
        public DbSet<Upload> Uploads { get; set; }
        public DbSet<UploadCourse> UploadsCourse { get; set; }
        public DbSet<UploadType> UploadsType { get; set; }
        public DbSet<WorkContract> WorkContracts { get; set; }
        public DbSet<TeacherCourse> TeacherCourse { get; set; }

        public DbSet<Location> Location { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}
