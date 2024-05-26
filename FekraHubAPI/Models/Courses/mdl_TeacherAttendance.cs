using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.Models.Courses
{
    public class Mdl_TeacherAttendance : IMapFrom<TeacherAttendance>
    {
        public DateTime Date { get; set; }
        public int? CourseID { get; set; }
        public string? TeacherID { get; set; }
        public int? StatusID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Mdl_TeacherAttendance, TeacherAttendance>();
        }
    }
}
