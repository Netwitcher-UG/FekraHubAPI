using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using FekraHubAPI.Data.Models;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_CourseSchedule : IMapFrom<CourseSchedule>
    {

        public string DayOfWeek { get; set; }



        public string StartTime { get; set; }


        public string EndTime { get; set; }

        public int? CourseID { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_CourseSchedule, CourseSchedule>();
        }

    }
}
