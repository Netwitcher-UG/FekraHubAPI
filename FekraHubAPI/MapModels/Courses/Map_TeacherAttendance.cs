using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_TeacherAttendance : IMapFrom<TeacherAttendance>
    {
        public DateTime Date { get; set; }
        public int? CourseID { get; set; }
        public string? TeacherID { get; set; }
        public int? StatusID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_TeacherAttendance, TeacherAttendance>();
        }
    }
}
