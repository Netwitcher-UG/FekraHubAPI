using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.Models.Courses
{
    public class mdl_UploadCourse: IMapFrom<UploadCourse>
    {
        public int? UploadID { get; set; }
        public int? CourseID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<mdl_UploadCourse, UploadCourse>();
        }
    }
}
