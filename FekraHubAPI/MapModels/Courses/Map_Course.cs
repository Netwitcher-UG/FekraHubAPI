using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Course : IMapFrom<Course>
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Lessons must be numeric.")]
        public int Lessons { get; set; }
        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Capacity must be numeric.")]
        public int Capacity { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int RoomId { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Course, Course>();
        }
    }
}
