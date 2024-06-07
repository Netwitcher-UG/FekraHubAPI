using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Report
    {
        public string data { get; set; }

        public bool? Improved { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string? UserId { get; set; }
        public int? StudentId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Report, Report>();
        }
    }
}
