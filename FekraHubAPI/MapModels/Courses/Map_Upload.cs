using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Upload : IMapFrom<Upload>
    {
        [Required]
        public byte[] file { get; set; }

        public int? UploadTypeid { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Upload, Upload>();
        }

    }
}
