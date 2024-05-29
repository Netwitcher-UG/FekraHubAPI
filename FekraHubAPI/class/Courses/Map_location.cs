using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.@class.Courses
{
    public class Map_location : IMapFrom<Location>
    {

        public string Street { get; set; }
        public string StreetNr { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_location, Location>();
        }

    }
}
