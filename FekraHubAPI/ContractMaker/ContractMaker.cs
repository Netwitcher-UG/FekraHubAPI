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
        public async Task ConverterHtmlToPdf(Student student)
        {
            try
            {
                HtmlToPdf HtmlToPdf = new HtmlToPdf();
                var contractPages = await ContractHtmlPage(student, student.ParentID);
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
                    StudentID = student.Id,
                    File = pdf,
                    CreationDate = DateTime.Now
                };
                await _repo.Add(studentContract);
            }
            catch (Exception ex) 
            {
                if (await _studentrepo.IDExists(student.Id)) 
                {
                    await _studentrepo.Delete(student.Id);
                }
                if ((await _repo.GetRelation()).Where(x => x.StudentID == student.Id).Any())
                {
                    await _repo.Delete(student.Id);
                }
            }
            

        }
        public async Task<byte[]> GetContractPdf(int studentId)
        {
            var AllContracts = await _repo.GetAll();
            var contract = AllContracts.Where(c => c.StudentID == studentId).First();
            return contract.File;
        }
        public async Task<List<string>> ContractHtml(Student student, string parentId)//
        {
            return await ContractHtmlPage(student, parentId);
        }

        private async Task<List<string>> ContractHtmlPage(Student student, string parentId)//
        {
            var schoolInfo = (await _schoolInforepo.GetRelation()).SingleOrDefault();
            var parent = await _Usersrepo.GetUser(parentId);
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
