using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.Models.Courses
{
    public class mdl_EventType : IMapFrom<EventType>
    {

        public string TypeTitle { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<mdl_EventType, EventType>();
        }
    }
}
