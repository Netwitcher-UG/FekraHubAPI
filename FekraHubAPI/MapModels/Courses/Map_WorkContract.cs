using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_WorkContract : IMapFrom<WorkContract>


    {
        public byte[] file { get; set; }

        public string UserId { get; set; }

    }
}
