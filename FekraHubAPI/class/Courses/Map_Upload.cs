using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.@class.Courses
{
    public class Map_Upload : IMapFrom<Upload>
    {

        public byte[] file { get; set; }

        public int? UploadTypeID { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Upload, Upload>();
        }

    }
}
