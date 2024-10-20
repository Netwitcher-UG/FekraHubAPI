using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;
namespace FekraHubAPI.MapModels.Courses
{
    public class Map_StudentAttendance 
    {
        [Required]
        public int StudentID { get; set; }
        [Required]
        public int StatusID { get; set; }
    }
}
