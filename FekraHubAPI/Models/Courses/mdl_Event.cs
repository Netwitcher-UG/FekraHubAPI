using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.Models.Courses
{
    public class mdl_Event : IMapFrom<Event>
    {
        public string EventName { get; set; }
        public DateTime Date { get; set; }

        public int? TypeID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<mdl_Event, Event>();
        }

    }
}
