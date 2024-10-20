using AutoMapper;
using FekraHubAPI.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.MapModels.Courses
{
    public class Map_PayRoll : IMapFrom<PayRoll>
    {

        [Required]
        public byte[] File { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? UserID { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_PayRoll, PayRoll>();
        }
    }
}
