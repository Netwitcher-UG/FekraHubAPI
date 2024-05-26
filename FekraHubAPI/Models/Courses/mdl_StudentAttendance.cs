using AutoMapper;
using FekraHubAPI.Data.Models;
namespace FekraHubAPI.Models.Courses
{
    public class Mdl_StudentAttendance : IMapFrom<StudentAttendance>
    {
        public DateTime Date { get; set; }
        public int? CourseID { get; set; }
        public int? StudentID { get; set; }
        public int? StatusID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Mdl_StudentAttendance, StudentAttendance>();
        }
    }
}
