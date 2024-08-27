﻿using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolInfoController : ControllerBase
    {
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<SchoolInfoController> _logger;
        public SchoolInfoController(IRepository<SchoolInfo> schoolInfoRepo,IMapper mapper,
            ILogger<SchoolInfoController> logger)
        {
            _schoolInfoRepo = schoolInfoRepo;
            _mapper = mapper;
            _logger = logger;
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpGet]
        public async Task<IActionResult> GetSchoolInfo()
        {
            try
            {
                var schoolInfo = (await _schoolInfoRepo.GetAll()).FirstOrDefault();
                if (schoolInfo == null)
                {
                    return NotFound();
                }
                var result = new
                {
                    schoolInfo.Id,
                    schoolInfo.SchoolName,
                    schoolInfo.SchoolOwner,
                    schoolInfo.UrlDomain,
                    schoolInfo.EmailServer,
                    schoolInfo.EmailPortNumber,
                    schoolInfo.FromEmail,
                    schoolInfo.Password,
                    schoolInfo.LogoBase64,
                    schoolInfo.PrivacyPolicy,
                    StudentsReportsKeys = schoolInfo.StudentsReportsKeys.Select(x => new
                    {
                        x.Id,
                        x.Keys
                    }),
                    ContractPages = schoolInfo.ContractPages.Select(x => new
                    {
                        x.Id,
                        x.ConPage
                    }),

                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPost]
        public async Task<IActionResult> InsertSchoolInfo([FromForm] Map_SchoolInfo SchoolInfo)
        {
            try
            {
                var schoolInfos = await _schoolInfoRepo.GetAll();
                if (schoolInfos.Any())
                {
                    return BadRequest("School Information was added earlier");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(SchoolInfo);
                }
                var schoolInfo = _mapper.Map<SchoolInfo>(SchoolInfo);
                if (SchoolInfo.ContractPages != null && SchoolInfo.ContractPages.Any())
                {
                    schoolInfo.ContractPages = SchoolInfo.ContractPages
                        .Select(page => new ContractPage
                        {
                            ConPage = page,
                            SchoolInfo = schoolInfo
                        }).ToList();
                }

                if (SchoolInfo.StudentsReportsKeys != null && SchoolInfo.StudentsReportsKeys.Any())
                {
                    schoolInfo.StudentsReportsKeys = SchoolInfo.StudentsReportsKeys
                        .Select(key => new StudentsReportsKey
                        {
                            Keys = key,
                            SchoolInfo = schoolInfo
                        }).ToList();
                }
                await _schoolInfoRepo.Add(schoolInfo);
                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPut]
        public async Task<IActionResult> UpdateSchoolInfo([FromForm] Map_SchoolInfo schoolInfo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(schoolInfo);
                }

                var schoolInfos = (await _schoolInfoRepo.GetRelation<SchoolInfo>()).FirstOrDefault();
                if (schoolInfos == null)
                {
                    return BadRequest("No school Information added");
                }

                _mapper.Map(schoolInfo, schoolInfos);

                if (schoolInfo.ContractPages != null)
                {
                    schoolInfos.ContractPages.Clear();

                    schoolInfos.ContractPages = schoolInfo.ContractPages
                        .Select(page => new ContractPage
                        {
                            ConPage = page,
                            SchoolInfoId = schoolInfos.Id
                        }).ToList();
                }

                if (schoolInfo.StudentsReportsKeys != null)
                {
                    schoolInfos.StudentsReportsKeys.Clear();

                    schoolInfos.StudentsReportsKeys = schoolInfo.StudentsReportsKeys
                        .Select(key => new StudentsReportsKey
                        {
                            Keys = key,
                            SchoolInfoId = schoolInfos.Id
                        }).ToList();
                }

                await _schoolInfoRepo.Update(schoolInfos);

                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSchoolInfo()
        {
            try
            {
                var schoolInfos = await _schoolInfoRepo.GetAll();
                foreach (var schoolInfo in schoolInfos)
                {
                    if (schoolInfo.ContractPages != null)
                    {
                        schoolInfo.ContractPages.Clear();
                    }

                    if (schoolInfo.StudentsReportsKeys != null)
                    {
                        schoolInfo.StudentsReportsKeys.Clear();
                    }

                    await _schoolInfoRepo.Delete(schoolInfo.Id);
                }

                return Ok("Done");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "SchoolInfoController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [AllowAnonymous]
        [HttpGet("SchoolLogo")]
        public async Task<IActionResult> SchoolLogo()
        {
            try
            {
                var logoBase64 = (await _schoolInfoRepo.GetRelation<SchoolInfo>()).First().LogoBase64;
                var imageBytes = Convert.FromBase64String(logoBase64 ?? "");
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
