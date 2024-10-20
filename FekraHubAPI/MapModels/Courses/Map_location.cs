using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_location : IMapFrom<Location>
    {
        [Required]
        public string Name { get; set; }
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
