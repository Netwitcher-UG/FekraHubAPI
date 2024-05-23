using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Models.Courses
{
    public class mdl_Course  : IMapFrom<Course>
    {
        [StringLength(50)]
        public string Name { get; set; }
        public int Price { get; set; }
        public int Lessons { get; set; }
        public int Capacity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public string? UserId { get; set; }
        public int? RoomId { get; set; }



        public void Mapping(Profile profile)
        {
            profile.CreateMap<mdl_Course, Course>();
        }
    }
}
