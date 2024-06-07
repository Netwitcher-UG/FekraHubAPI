using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SelectPdf;
using System.Drawing;

namespace FekraHubAPI.ContractMaker
{
    public class ContractMaker : IContractMaker
    {
        private readonly IRepository<StudentContract> _repo;
        private readonly IRepository<ApplicationUser> _Usersrepo;
        private readonly IRepository<Student> _studentrepo;
        private readonly IRepository<SchoolInfo> _schoolInforepo;
        public ContractMaker(IRepository<StudentContract> repo, IRepository<Student> studentrepo,
            IRepository<ApplicationUser> Usersrepo, IRepository<SchoolInfo> schoolInforepo)
        {
            _repo = repo;
            _studentrepo = studentrepo;
            _Usersrepo = Usersrepo;
            _schoolInforepo = schoolInforepo;
        }
        public async Task ConverterHtmlToPdf(int studentId)
        {
            var contracts = await _repo.GetRelation();
            var isExists = contracts.Where(s => s.StudentID == studentId).ToList();
            if (!isExists.Any())
            {
                HtmlToPdf HtmlToPdf = new HtmlToPdf();
                var contractPages = await ContractHtmlPage(studentId);
                PdfDocument finalPdf = new PdfDocument();
                foreach (var contractPage in contractPages)
                {
                    PdfDocument pdfDocument = HtmlToPdf.ConvertHtmlString(contractPage);
                    finalPdf.Append(pdfDocument);

                }
                byte[] pdf = finalPdf.Save();
                finalPdf.Close();
                StudentContract studentContract = new()
                {
                    StudentID = studentId,
                    File = pdf,
                    CreationDate = DateTime.Now
                };
                await _repo.Add(studentContract);
            }
        }
        public async Task<byte[]> GetContractPdf(int studentId)
        {
            var AllContracts = await _repo.GetAll();
            var contract = AllContracts.Where(c => c.StudentID == studentId).First();
            return contract.File;
        }
        public async Task<List<string>> ContractHtml(int studentId)
        {
            return await ContractHtmlPage(studentId);
        }

        private async Task<List<string>> ContractHtmlPage(int studentID)
        {
            var schoolInfo = (await _schoolInforepo.GetRelation()).SingleOrDefault();
            var student = await _studentrepo.GetById(studentID);
            var parent = await _Usersrepo.GetUser(student.ParentID);
            List<string> contractPages = schoolInfo.ContractPages;

            contractPages[0] = contractPages[0]
                .Replace("{student.FirstName}", student.FirstName)
                .Replace("{student.LastName}", student.LastName)
                .Replace("{student.Birthday.Date.ToString('yyyy-MM-dd')}", student.Birthday.Date.ToString("yyyy-MM-dd"))
                .Replace("{student.Nationality}", student.Nationality)
                .Replace("{parent.FirstName}", parent.FirstName)
                .Replace("{parent.LastName}", parent.LastName)
                .Replace("{parent.Street}", parent.Street)
                .Replace("{parent.StreetNr}", parent.StreetNr)
                .Replace("{parent.ZipCode}", parent.ZipCode)
                .Replace("{parent.EmergencyPhoneNumber}", parent.EmergencyPhoneNumber)
                .Replace("{parent.PhoneNumber}", parent.PhoneNumber)
                .Replace("{parent.Email}", parent.Email);
            for (var i = 0; i < contractPages.Count(); i++)
            {
                contractPages[i] = contractPages[i].Replace("{fekrahublogo}", schoolInfo.LogoBase64);
            }
            return contractPages;
        }
    }
}
