using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_UploadCourse : IMapFrom<UploadCourse>
    {
        public int? UploadID { get; set; }
        public int? CourseID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_UploadCourse, UploadCourse>();
        }
    }
}
