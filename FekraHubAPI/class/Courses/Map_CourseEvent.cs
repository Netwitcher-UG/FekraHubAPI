using AutoMapper;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class Map_CourseEvent : IMapFrom<CourseEvent>
    {
    
        public int? ScheduleID { get; set; }

        public int? EventID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_CourseEvent, CourseEvent>();
        }
    }
}
