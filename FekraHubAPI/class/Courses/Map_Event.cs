using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.Models.Courses
{
    public class Map_Event : IMapFrom<Event>
    {
        public string EventName { get; set; }
        public DateTime Date { get; set; }

        public int? TypeID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Event, Event>();
        }

    }
}
