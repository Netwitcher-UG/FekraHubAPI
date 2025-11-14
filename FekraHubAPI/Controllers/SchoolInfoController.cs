using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.SchoolInfo;
using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FekraHubAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SchoolInfoController : ControllerBase
    {
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly IRepository<StudentsReportsKey> _studentReportKeys;
        private readonly ILogger<SchoolInfoController> _logger;
        public SchoolInfoController(IRepository<SchoolInfo> schoolInfoRepo, 
            ILogger<SchoolInfoController> logger,
            IRepository<StudentsReportsKey> studentReportKeys)
        {
            _schoolInfoRepo = schoolInfoRepo;
            _logger = logger;
            _studentReportKeys = studentReportKeys;
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpGet("SchoolInfoBasic")]
        public async Task<IActionResult> GetSchoolInfoBasic()
        {
            try
            {
                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                    selector: x => new { x.SchoolName, x.SchoolOwner, x.LogoBase64,x.Facebook,x.Instagram,x.PrivacyPolicy },
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true);
                

                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpGet("SchoolInfoEmailSender")]
        public async Task<IActionResult> GetSchoolInfoEmailSender()
        {
            try
            {
                
                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                    selector: x => new { x.EmailServer, x.EmailPortNumber, x.FromEmail, x.Password },
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);
                

                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpGet("GetSchoolInfoReportKeys")]
        public async Task<IActionResult> GetSchoolInfoReportKeys()
        {
            try
            {
                
                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                    include:x=>x.Include(k=>k.StudentsReportsKeys),
                    selector: x => x.StudentsReportsKeys.Select(z => z.Keys).ToList(),
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);
                

                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        //[Authorize(Policy = "ManageSchoolInfo")]
        //[HttpGet("SchoolInfoContractAndPolicy")]
        //public async Task<IActionResult> GetSchoolInfoContractAndPolicy()
        //{
        //    try
        //    {
                
        //        var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
        //            include: x => x.Include(k => k.ContractPages),
        //            selector: x => new { x.PrivacyPolicy, contractPages = x.ContractPages.Select(z => z.ConPage).ToList() },
        //            returnType: QueryReturnType.SingleOrDefault,
        //            asNoTracking: true);
                
        //        return Ok(schoolInfo);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
        //        return BadRequest(ex.Message);
        //    }

        //}
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPost("SchoolInfo_Basic")]
        public async Task<IActionResult> InsertSchoolInfoBasic([FromForm] Map_SchoolInfo_Basic schoolInfo_Basic)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(schoolInfo_Basic);
                }
                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(selector:x=>x);
                if (schoolInfo == null)
                {
                    return BadRequest("School Info not found");
                }
                if (schoolInfo_Basic.Logo != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        schoolInfo_Basic.Logo.CopyTo(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        schoolInfo.LogoBase64 = Convert.ToBase64String(fileBytes);
                    }
                }
                if (!string.IsNullOrEmpty(schoolInfo_Basic.SchoolName))
                {
                    schoolInfo.SchoolName = schoolInfo_Basic.SchoolName;
                }
                if (!string.IsNullOrEmpty(schoolInfo_Basic.SchoolOwner))
                {
                    schoolInfo.SchoolOwner = schoolInfo_Basic.SchoolOwner;
                }
                if (!string.IsNullOrEmpty(schoolInfo_Basic.FacebookLink))
                {
                    schoolInfo.Facebook = schoolInfo_Basic.FacebookLink;
                }
                if (!string.IsNullOrEmpty(schoolInfo_Basic.InstagramLink))
                {
                    schoolInfo.Instagram = schoolInfo_Basic.InstagramLink;
                }
                if (!string.IsNullOrEmpty(schoolInfo_Basic.PrivacyPolicyLink))
                {
                    schoolInfo.PrivacyPolicy = schoolInfo_Basic.PrivacyPolicyLink;
                }

                await _schoolInfoRepo.Update(schoolInfo);
                return Ok("Success");


            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPost("SchoolInfo_EmailSender")]
        public async Task<IActionResult> InsertSchoolInfoEmailSender([FromForm] Map_schoolInfo_EmailSender schoolInfo_EmailSender)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(schoolInfo_EmailSender);
                }
                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(selector:x=>x);
                if (schoolInfo == null)
                {
                    return BadRequest("Sie können keine E-Mail-Absenderinformationen hinzufügen, bevor die Basisinformationen hinzugefügt wurden.");//You cant add email sender info before adding the basic info
                }
                if (!string.IsNullOrEmpty(schoolInfo.EmailServer))
                {
                    schoolInfo.EmailServer = schoolInfo_EmailSender.EmailServer;
                }
                if (!string.IsNullOrEmpty(schoolInfo.FromEmail))
                {
                    schoolInfo.FromEmail = schoolInfo_EmailSender.FromEmail;
                }
                if (!string.IsNullOrEmpty(schoolInfo.Password))
                {
                    schoolInfo.Password = schoolInfo_EmailSender.Password;
                }
                if(schoolInfo_EmailSender.EmailPortNumber != 0)
                {
                    schoolInfo.EmailPortNumber = schoolInfo_EmailSender.EmailPortNumber ?? 587;
                }
                await _schoolInfoRepo.Update(schoolInfo);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPost("SchoolInfo_ReportKeys")]
        public async Task<IActionResult> InsertSchoolInfoReportKeys([FromForm] Map_SchoolInfo_ReportKeys schoolInfo_ReportKeys)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(schoolInfo_ReportKeys);
                }
                if(schoolInfo_ReportKeys.StudentsReportsKeys == null || schoolInfo_ReportKeys.StudentsReportsKeys.Count == 0)
                {
                    return BadRequest("You need to add some keys for the reports");
                }

                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                        selector: x => new { x.Id },
                        returnType: QueryReturnType.Single
                        );
                if (schoolInfo == null)
                {
                    return Ok("Sie können keine Berichtsschlüssel hinzufügen, bevor die Basisinformationen hinzugefügt wurden.");//You cant add report keys before adding the basic info

                }

                var existingKeys = await _studentReportKeys.GetRelationList(
                    where: k => k.SchoolInfoId == schoolInfo.Id,
                    selector:x=>x
                    );
                existingKeys =  existingKeys.OrderBy(k => k.Id).ToList();

                 
                    

                var incomingKeys = schoolInfo_ReportKeys.StudentsReportsKeys;
                var minCount = Math.Min(existingKeys.Count, incomingKeys.Count);

                for (int i = 0; i < minCount; i++)
                {
                    if (!string.Equals(existingKeys[i].Keys, incomingKeys[i], StringComparison.Ordinal))
                    {
                        existingKeys[i].Keys = incomingKeys[i];
                        await _studentReportKeys.Update(existingKeys[i]);
                    }
                }

                if (incomingKeys.Count > existingKeys.Count)
                {
                    for (int i = existingKeys.Count; i < incomingKeys.Count; i++)
                    {
                        var newKey = new StudentsReportsKey
                        {
                            Keys = incomingKeys[i],
                            SchoolInfoId = schoolInfo.Id
                        };
                        await _studentReportKeys.Add(newKey);
                    }
                }

                if (existingKeys.Count > incomingKeys.Count)
                {
                    var toDelete = existingKeys.Skip(incomingKeys.Count).ToList();
                    _studentReportKeys.DeleteRange(toDelete);
                }

                return Ok("Success");





            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        //[Authorize(Policy = "ManageSchoolInfo")]
        //[HttpPost("SchoolInfo_ContractAndPolicy")]
        //public async Task<IActionResult> InsertSchoolInfoContractAndPolicy([FromForm] Map_SchoolInfo_ContractAndPolicy schoolInfo_ContractAndPolicy)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(schoolInfo_ContractAndPolicy);
        //        }


        //        bool SchoolInfoExist = await _schoolInfoRepo.DataExist();
        //        if (SchoolInfoExist)
        //        {
        //            var OldSchoolInfo = await _schoolInfoRepo.GetRelationSingle(
        //                include: x => x.Include(k => k.ContractPages),
        //                selector: x => x,
        //                returnType: QueryReturnType.Single
        //                );


        //            OldSchoolInfo.ContractPages.Clear();
        //            List<ContractPage> studentsContractPages = new List<ContractPage>();
        //            foreach (var page in schoolInfo_ContractAndPolicy.ContractPages)
        //            {
        //                var studentRKey = new ContractPage
        //                {
        //                    ConPage = page,
        //                    SchoolInfoId = OldSchoolInfo.Id
        //                };
        //                studentsContractPages.Add(studentRKey);
        //            }
        //            OldSchoolInfo.ContractPages = studentsContractPages;
        //            await _schoolInfoRepo.Update(OldSchoolInfo);
        //            return Ok("Success");

        //        }
        //        else
        //        {
        //            return Ok("Sie können keinen Vertrag und keine Richtlinie hinzufügen, bevor die Basisinformationen hinzugefügt wurden.");//You cant add contract and policy before adding the basic info

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
        //        return BadRequest(ex.Message);
        //    }

        //}
        //[Authorize(Policy = "ManageSchoolInfo")]
        //[HttpPut]
        //public async Task<IActionResult> UpdateSchoolInfo([FromForm] Map_SchoolInfo_Basic schoolInfo)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(schoolInfo);
        //        }

        //        var schoolInfos = (await _schoolInfoRepo.GetRelation<SchoolInfo>()).FirstOrDefault();
        //        if (schoolInfos == null)
        //        {
        //            return BadRequest("No school Information added");
        //        }

        //        _mapper.Map(schoolInfo, schoolInfos);

        //        if (schoolInfo.ContractPages != null)
        //        {
        //            schoolInfos.ContractPages.Clear();

        //            schoolInfos.ContractPages = schoolInfo.ContractPages
        //                .Select(page => new ContractPage
        //                {
        //                    ConPage = page,
        //                    SchoolInfoId = schoolInfos.Id
        //                }).ToList();
        //        }

        //        if (schoolInfo.StudentsReportsKeys != null)
        //        {
        //            schoolInfos.StudentsReportsKeys.Clear();

        //            schoolInfos.StudentsReportsKeys = schoolInfo.StudentsReportsKeys
        //                .Select(key => new StudentsReportsKey
        //                {
        //                    Keys = key,
        //                    SchoolInfoId = schoolInfos.Id
        //                }).ToList();
        //        }

        //        await _schoolInfoRepo.Update(schoolInfos);

        //        return Ok(schoolInfo);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
        //        return BadRequest(ex.Message);
        //    }

        //}
        //[Authorize(Policy = "ManageSchoolInfo")]
        //[HttpDelete]
        //public async Task<IActionResult> DeleteSchoolInfo()
        //{
        //    try
        //    {
        //        var schoolInfos = await _schoolInfoRepo.GetAll();
        //        foreach (var schoolInfo in schoolInfos)
        //        {
        //            if (schoolInfo.ContractPages != null)
        //            {
        //                schoolInfo.ContractPages.Clear();
        //            }

        //            if (schoolInfo.StudentsReportsKeys != null)
        //            {
        //                schoolInfo.StudentsReportsKeys.Clear();
        //            }

        //            await _schoolInfoRepo.Delete(schoolInfo.Id);
        //        }

        //        return Ok("Done");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
        //        return BadRequest(ex.Message);
        //    }

        //}
        [AllowAnonymous]
        [HttpGet("SchoolLogo1")]
        public async Task<IActionResult> SchoolLogo1()
        {
            try
            {
                var logoBase64 = await _schoolInfoRepo.GetRelationSingle(
                    selector:x=>x.LogoBase64,
                    asNoTracking:true
                    );
                var imageBytes = Convert.FromBase64String(logoBase64 ?? "");
                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [AllowAnonymous]
        [HttpGet("SchoolLogo2")]
        public IActionResult SchoolLogo2()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(),"Controllers", "1.png");

                var imageBytes = System.IO.File.ReadAllBytes(filePath);

                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [AllowAnonymous]
        [HttpGet("SchoolLogo3")]
        public IActionResult SchoolLogo3()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "3.png");

                var imageBytes = System.IO.File.ReadAllBytes(filePath);

                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [AllowAnonymous]
        [HttpGet("SchoolLogo4")]
        public IActionResult InstagramLogo()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "instagram.png");

                var imageBytes = System.IO.File.ReadAllBytes(filePath);

                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [AllowAnonymous]
        [HttpGet("SchoolLogo5")]
        public IActionResult FacebookLogo()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "facebook.png");

                var imageBytes = System.IO.File.ReadAllBytes(filePath);

                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        //[AllowAnonymous]
        //[HttpPut("updateTESTING")]
        //public async Task<IActionResult> UpdateFeildsSchoolInfo(string domain)
        //{
        //    var schoolInfo = (await _schoolInfoRepo.GetRelation()).First();
        //    schoolInfo.UrlDomain = domain;
        //    await _schoolInfoRepo.Update(schoolInfo);
        //    return Ok(schoolInfo.UrlDomain);
        //}
    }
}
