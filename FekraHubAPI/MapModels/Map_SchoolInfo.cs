using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.MapModels
{
    public class Map_SchoolInfo : IMapFrom<SchoolInfo>
    {
        [MaxLength(50)]
        public string UrlDomain { get; set; }
        [MaxLength(50)]
        public string SchoolName { get; set; }
        [MaxLength(50)]
        public string ShoolOwner { get; set; }
        public string LogoBase64 { get; set; }
        public List<string> StudentsReportsKeys { get; set; }
        [MaxLength(50)]
        public string EmailServer { get; set; }
        public int EmailPortNumber { get; set; }
        [MaxLength(50)]
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public List<string> ContractPages { get; set; }
        public string PrivacyPolicy { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_SchoolInfo, SchoolInfo>();
        }
    }
}
