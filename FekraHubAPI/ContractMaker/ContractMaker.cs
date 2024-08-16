using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;

using DinkToPdf;
using DinkToPdf.Contracts;
using System.Runtime.ConstrainedExecution;
using System;

namespace FekraHubAPI.ContractMaker
{
    public class ContractMaker : IContractMaker
    {
        private readonly IRepository<StudentContract> _repo;
        private readonly IRepository<ApplicationUser> _Usersrepo;
        private readonly IRepository<Student> _studentrepo;
        private readonly IRepository<SchoolInfo> _schoolInforepo;
        private readonly IRepository<ContractPage> _contractPagesrepo;
        private readonly IConverter _converter;
        public ContractMaker(IRepository<StudentContract> repo, IRepository<Student> studentrepo,
            IRepository<ApplicationUser> Usersrepo, IRepository<SchoolInfo> schoolInforepo, IConverter converter, IRepository<ContractPage> contractPagesrepo)
        {
            _repo = repo;
            _studentrepo = studentrepo;
            _Usersrepo = Usersrepo;
            _schoolInforepo = schoolInforepo;
            _converter = converter;
            _contractPagesrepo = contractPagesrepo;
        }
        private async Task<byte[]> PdfFile(Student student)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 0, Bottom = 0, Right = 0, Left = 0 },
                DocumentTitle = "Generated PDF"
            };
            var objectSettings1 = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = (await ContractHtmlPage(student))[0],
                WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true, LoadImages = true },
            };
            var objectSettings2 = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = (await ContractHtmlPage(student))[1],
                WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true, LoadImages = true },
            };
            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings1,objectSettings2 }
            };
            byte[] pdfFile = _converter.Convert(document);
            return pdfFile;
        }
        public async Task ConverterHtmlToPdf(Student student)
        {
            try
            {
                byte[] pdfFile = await PdfFile(student);
                StudentContract studentContract = new()
                {
                    StudentID = student.Id,
                    File = pdfFile,
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
        public async Task<string> ContractHtml(Student student)
        {
            byte[] x = await PdfFile(student);
            return Convert.ToBase64String(x);
        }

        private async Task<List<string>> ContractHtmlPage(Student student)
        {
            var schoolInfoLogo = (await _schoolInforepo.GetRelation()).Select(x=> x.LogoBase64).SingleOrDefault();
            if (schoolInfoLogo == null)
            {
                return new List<string>();
            }
            var parent = await _Usersrepo.GetUser(student.ParentID ?? "");
            if (parent == null)
            {
                return new List<string>();
            }
            List<string> contractPages = (await _contractPagesrepo.GetAll()).Select(x => x.ConPage).ToList();

            contractPages[0] = contractPages[0]
                .Replace("{student.FirstName}", student.FirstName ?? "")
                .Replace("{student.LastName}", student.LastName ?? "")
                .Replace("{student.Birthday.Date.ToString('yyyy-MM-dd')}", student.Birthday.Date.ToString("yyyy-MM-dd"))
                .Replace("{student.Nationality}", student.Nationality ?? "")
                .Replace("{parent.FirstName}", parent.FirstName ?? "")
                .Replace("{parent.LastName}", parent.LastName ?? "")
                .Replace("{parent.Street}", parent.Street ?? "")
                .Replace("{parent.StreetNr}", parent.StreetNr ?? "")
                .Replace("{parent.ZipCode}", parent.ZipCode ?? "")
                .Replace("{parent.EmergencyPhoneNumber}", parent.EmergencyPhoneNumber ?? "")
                .Replace("{parent.PhoneNumber}", parent.PhoneNumber ?? "")
                .Replace("{parent.Email}", parent.Email ?? "");
            for (var i = 0; i < contractPages.Count(); i++)
            {
                contractPages[i] = contractPages[i].Replace("{fekrahublogo}", schoolInfoLogo ?? "");
            }
            return contractPages;
        }
    }

}
