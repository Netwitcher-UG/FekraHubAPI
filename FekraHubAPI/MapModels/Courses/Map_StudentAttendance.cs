using AutoMapper;
using FekraHubAPI.Data.Models;
namespace FekraHubAPI.MapModels.Courses
{
    public class Map_StudentAttendance : IMapFrom<StudentAttendance>
    {
        public DateTime Date { get; set; }
        public int CourseID { get; set; }
        public int StudentID { get; set; }
        public int StatusID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_StudentAttendance, StudentAttendance>();
        }
    }
}
