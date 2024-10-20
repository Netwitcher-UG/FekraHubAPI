using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Report : IMapFrom<Report>
    {
        [Required]
        public string data { get; set; }

        public bool? Improved { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        [Required]
        public string UserId { get; set; }
        [Required]
        public int StudentId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Report, Report>();
        }
    }
}
