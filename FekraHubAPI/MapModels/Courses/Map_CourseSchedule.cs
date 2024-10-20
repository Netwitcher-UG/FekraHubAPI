using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_CourseSchedule : IMapFrom<CourseSchedule>
    {
        [Required]
        public string DayOfWeek { get; set; }


        [Required]
        public string StartTime { get; set; }

        [Required]
        public string EndTime { get; set; }

        public int? CourseID { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_CourseSchedule, CourseSchedule>();
        }

    }
}
