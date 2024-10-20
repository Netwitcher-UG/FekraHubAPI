using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_UploadType : IMapFrom<UploadType>
    {
        [Required]
        public string TypeTitle { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_UploadType, UploadType>();
        }

    }
}
