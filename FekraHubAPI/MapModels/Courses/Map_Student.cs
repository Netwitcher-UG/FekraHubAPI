using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_Student : IMapFrom<Student>
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime Birthday { get; set; }
        public string Nationality { get; set; }
        public string? Note { get; set; }
        public string? Street { get; set; }
        public string? StreetNr { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public int? CourseID { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_Student, Student>();
        }

    }
}
