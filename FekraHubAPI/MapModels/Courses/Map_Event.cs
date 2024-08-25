using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Event : IMapFrom<Event>
    {
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int? TypeID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Event, Event>();
        }

    }
}
