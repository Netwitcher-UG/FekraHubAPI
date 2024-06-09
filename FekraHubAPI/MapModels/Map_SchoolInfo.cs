using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;

namespace FekraHubAPI.MapModels
{
    public class Map_SchoolInfo : IMapFrom<SchoolInfo>
    {
        public string UrlDomain { get; set; }
        public string SchoolName { get; set; }
        public string ShoolOwner { get; set; }
        public string LogoBase64 { get; set; }
        public List<string> StudentsReportsKeys { get; set; }
        public string EmailServer { get; set; }
        public int EmailPortNumber { get; set; }
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public List<string> ContractPages { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Map_SchoolInfo, SchoolInfo>();
        }
    }
}
