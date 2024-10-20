using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_TeacherAttendance 
    {
        public DateTime Date { get; set; }
        [Required]
        public string TeacherID { get; set; }
        [Required]
        public int StatusID { get; set; }

        
    }
}
