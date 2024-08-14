using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FekraHubAPI.Data.Models
{
    public class SchoolInfo
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string? UrlDomain { get; set; }
        [MaxLength(50)]
        public string? SchoolName { get; set; }
        [MaxLength(50)]
        public string? SchoolOwner { get; set; }
        public string? LogoBase64 { get; set; }
        [MaxLength(50)]
        public string? EmailServer { get; set; }
        public int EmailPortNumber { get; set; }
        [MaxLength(50)]
        public string? FromEmail { get; set; }
        public string? Password { get; set; }
        public string? PrivacyPolicy { get; set; }
        public ICollection<ContractPage> ContractPages { get; set; }
        public ICollection<StudentsReportsKey> StudentsReportsKeys { get; set; }
    }
    public class ContractPage
    {
        public int Id { get; set; } 
        public string ConPage {  get; set; }

        [ForeignKey("SchoolInfoId")]
        public int SchoolInfoId { get; set; } 
        public SchoolInfo SchoolInfo { get; set; }  
    }
    public class StudentsReportsKey
    {
        public int Id { get; set; }
        public string Keys { get; set; }

        [ForeignKey("SchoolInfoId")]
        public int SchoolInfoId { get; set; }
        public SchoolInfo SchoolInfo { get; set; }
    }

}
