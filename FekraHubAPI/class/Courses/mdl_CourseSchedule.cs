using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using FekraHubAPI.Models.Courses;

namespace FekraHubAPI.Data.Models
{
    public class mdl_CourseSchedule : IMapFrom<CourseSchedule>
    {
       
        public string DayOfWeek { get; set; }

      
      
        public string StartTime { get; set; }

       
        public string EndTime { get; set; }

        public int? CourseID { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<mdl_CourseSchedule, CourseSchedule>();
        }

    }
}
