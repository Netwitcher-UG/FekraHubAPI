using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Event : IMapFrom<Event>
    {
        [Required]
        public string EventName { get; set; }
        
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

       

        public int? TypeID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Event, Event>();
        }

    }
}
