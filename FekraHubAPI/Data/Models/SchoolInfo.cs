using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Data.Models
{
    public class SchoolInfo
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string UrlDomain { get; set; }
        [MaxLength(50)]
        public string SchoolName { get; set; }
        [MaxLength(50)]
        public string SchoolOwner { get; set; }
        public string LogoBase64 { get; set; }
        public List<string> StudentsReportsKeys { get; set; }
        [MaxLength(50)]
        public string EmailServer { get; set; }
        public int EmailPortNumber { get; set; }
        [MaxLength(50)]
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public List<string> ContractPages { get; set; }
    }
}
