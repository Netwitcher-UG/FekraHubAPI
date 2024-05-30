using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Room : IMapFrom<Room>
    {
        [StringLength(50)]
        public string Name { get; set; }

        public int? LocationID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Room, Room>();
        }
    }
}
