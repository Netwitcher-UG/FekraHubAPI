using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.SchoolInfo;

using FekraHubAPI.Repositories.Interfaces;
using FekraHubAPI.Seeds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FekraHubAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SchoolInfoController : ControllerBase
    {
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<SchoolInfoController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public SchoolInfoController(IRepository<SchoolInfo> schoolInfoRepo, IMapper mapper,
            ILogger<SchoolInfoController> logger, UserManager<ApplicationUser> userManager)
        {
            _schoolInfoRepo = schoolInfoRepo;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpGet("SchoolInfoBasic")]
        public async Task<IActionResult> GetSchoolInfoBasic()
        {
            try
            {
                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                    selector: x => new { x.SchoolName, x.SchoolOwner, x.LogoBase64 },
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true);
                if (schoolInfo == null)
                {
                    return NotFound();
                }

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
                if (schoolInfo == null)
                {
                    return NotFound();
                }

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
                if (schoolInfo == null)
                {
                    return NotFound();
                }

                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpGet("SchoolInfoContractAndPolicy")]
        public async Task<IActionResult> GetSchoolInfoContractAndPolicy()
        {
            try
            {
                
                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                    include: x => x.Include(k => k.ContractPages),
                    selector: x => new { x.PrivacyPolicy, contractPages = x.ContractPages.Select(z => z.ConPage).ToList() },
                    returnType: QueryReturnType.SingleOrDefault,
                    asNoTracking: true);
                if (schoolInfo == null)
                {
                    return NotFound();
                }
                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
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
                var IsEmailExists = await _userManager.FindByEmailAsync(schoolInfo_Basic.Email);
                if (IsEmailExists != null)
                {
                    return BadRequest($"Email {schoolInfo_Basic.Email} is already token.");
                }
                string RoleAdmin = DefaultRole.Admin;
                var normalizedEmail = schoolInfo_Basic.Email.ToUpperInvariant();
                var normalizedUserName = schoolInfo_Basic.Email.ToUpperInvariant();
                ApplicationUser admin = new()
                {
                    UserName = schoolInfo_Basic.Email,
                    Email = schoolInfo_Basic.Email,
                    NormalizedUserName = normalizedUserName,
                    FirstName = schoolInfo_Basic.FirstName,
                    LastName = schoolInfo_Basic.LastName,
                    NormalizedEmail = normalizedEmail,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                };

                IdentityResult result = await _userManager.CreateAsync(admin, schoolInfo_Basic.Password);
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(admin, RoleAdmin).Wait();
                }
                else
                {
                    return BadRequest(result.Errors.Select(x => x.Description).FirstOrDefault());
                }
                string LogoBase64 = "";
                using (var memoryStream = new MemoryStream())
                {
                    schoolInfo_Basic.Logo.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    LogoBase64 = Convert.ToBase64String(fileBytes);
                }
                bool SchoolInfoExist = await _schoolInfoRepo.DataExist();
                if (SchoolInfoExist)
                {
                    var OldSchoolInfo = (await _schoolInfoRepo.GetAll()).First();
                    OldSchoolInfo.SchoolName = schoolInfo_Basic.SchoolName;
                    OldSchoolInfo.SchoolOwner = schoolInfo_Basic.SchoolOwner;
                    OldSchoolInfo.LogoBase64 = LogoBase64;
                    await _schoolInfoRepo.Update(OldSchoolInfo);
                    return Ok("Success");

                }
                else
                {
                    //bool SchoolInfoBasicExist = await _schoolInfoRepo.DataExist(null,
                    //   new List<Expression<Func<SchoolInfo, bool>>>
                    //       {
                    //            entity => entity.SchoolName != null,
                    //            entity => entity.SchoolOwner !=null,
                    //            entity => entity.LogoBase64 != null
                    //       }
                    //   );
                    //if (SchoolInfoBasicExist)
                    //{
                    //    return BadRequest("School Basic Information was added earlier");
                    //}
                    SchoolInfo newSchoolInfo = new SchoolInfo()
                    {
                        SchoolName = schoolInfo_Basic.SchoolName,
                        SchoolOwner = schoolInfo_Basic.SchoolOwner,
                        LogoBase64 = LogoBase64
                    };
                    await _schoolInfoRepo.Add(newSchoolInfo);
                    return Ok("Success");

                }
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
                bool SchoolInfoExist = await _schoolInfoRepo.DataExist();
                if (SchoolInfoExist)
                {
                    var OldSchoolInfo = (await _schoolInfoRepo.GetAll()).First();
                    OldSchoolInfo.EmailServer = schoolInfo_EmailSender.EmailServer;
                    OldSchoolInfo.EmailPortNumber = schoolInfo_EmailSender.EmailPortNumber;
                    OldSchoolInfo.FromEmail = schoolInfo_EmailSender.FromEmail;
                    OldSchoolInfo.Password = schoolInfo_EmailSender.Password;
                    await _schoolInfoRepo.Update(OldSchoolInfo);
                    return Ok("Success");
                }
                else
                {
                    return Ok("You cant add email sender info before adding the basic info");
                }
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


                bool SchoolInfoExist = await _schoolInfoRepo.DataExist();
                if (SchoolInfoExist)
                {
                    var OldSchoolInfo = await _schoolInfoRepo.GetRelationSingle(
                        include:x=> x.Include(k=>k.StudentsReportsKeys),
                        selector: x => x,
                        returnType:QueryReturnType.Single
                        );


                    OldSchoolInfo.StudentsReportsKeys.Clear();
                    List<StudentsReportsKey> studentsReportsKeys = new List<StudentsReportsKey>();
                    foreach (var key in schoolInfo_ReportKeys.StudentsReportsKeys)
                    {
                        var studentRKey = new StudentsReportsKey
                        {
                            Keys = key,
                            SchoolInfoId = OldSchoolInfo.Id
                        };
                        studentsReportsKeys.Add(studentRKey);
                    }
                    OldSchoolInfo.StudentsReportsKeys = studentsReportsKeys;
                    await _schoolInfoRepo.Update(OldSchoolInfo);
                    return Ok("Success");

                }
                else
                {
                    return Ok("You cant add report keys before adding the basic info");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPost("SchoolInfo_ContractAndPolicy")]
        public async Task<IActionResult> InsertSchoolInfoContractAndPolicy([FromForm] Map_SchoolInfo_ContractAndPolicy schoolInfo_ContractAndPolicy)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(schoolInfo_ContractAndPolicy);
                }


                bool SchoolInfoExist = await _schoolInfoRepo.DataExist();
                if (SchoolInfoExist)
                {
                    var OldSchoolInfo = await _schoolInfoRepo.GetRelationSingle(
                        include: x => x.Include(k => k.ContractPages),
                        selector: x => x,
                        returnType: QueryReturnType.Single
                        );


                    OldSchoolInfo.ContractPages.Clear();
                    OldSchoolInfo.PrivacyPolicy = schoolInfo_ContractAndPolicy.PrivacyPolicy;
                    List<ContractPage> studentsContractPages = new List<ContractPage>();
                    foreach (var page in schoolInfo_ContractAndPolicy.ContractPages)
                    {
                        var studentRKey = new ContractPage
                        {
                            ConPage = page,
                            SchoolInfoId = OldSchoolInfo.Id
                        };
                        studentsContractPages.Add(studentRKey);
                    }
                    OldSchoolInfo.ContractPages = studentsContractPages;
                    await _schoolInfoRepo.Update(OldSchoolInfo);
                    return Ok("Success");

                }
                else
                {
                    return Ok("You cant add contract and policy before adding the basic info");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
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
                return File(imageBytes, "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        [AllowAnonymous]
        [HttpGet("SchoolLogo2")]
        public async Task<IActionResult> SchoolLogo2()
        {
            try
            {
                var logoBase64 = await _schoolInfoRepo.GetRelationSingle(
                    selector: x => x.LogoBase64,
                    asNoTracking: true
                    );
                var imageBytes = Convert.FromBase64String(logoBase64 ?? "");
                return File(imageBytes, "image/svg+xml");
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
