using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_WorkContract
    {
        [Required]
        public byte[] File { get; set; }

        public string? TeacherID { get; set; }

    }
}
