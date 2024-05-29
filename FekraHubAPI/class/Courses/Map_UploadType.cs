using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.@class.Courses
{
    public class Map_UploadType : IMapFrom<UploadType>


    {
        public string TypeTitle { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_UploadType, UploadType>();
        }

    }
}
