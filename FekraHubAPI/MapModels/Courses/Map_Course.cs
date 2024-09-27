using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Course : IMapFrom<Course>
    {
        [StringLength(50)]
        public string Name { get; set; }
        public int Price { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Lessons must be numeric.")]
        public int Lessons { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Capacity must be numeric.")]
        public int Capacity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int RoomId { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Course, Course>();
        }
    }
}
