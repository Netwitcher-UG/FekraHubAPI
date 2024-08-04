using AutoMapper;
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
        public SchoolInfoController(IRepository<SchoolInfo> schoolInfoRepo,IMapper mapper)
        {
            _schoolInfoRepo = schoolInfoRepo;
            _mapper = mapper;
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpGet]
        public async Task<IActionResult> GetSchoolInfo()
        {
            var schoolInfo = (await _schoolInfoRepo.GetAll()).FirstOrDefault();
            if(schoolInfo == null)
            {
                return NotFound();
            }
            return Ok(schoolInfo);
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPost]
        public async Task<IActionResult> InsertSchoolInfo([FromForm] Map_SchoolInfo SchoolInfo)
        {
            var schoolInfos = await _schoolInfoRepo.GetAll();
            if (schoolInfos.Any())
            {
                return BadRequest("School Information was added earlier");
            }
            if(!ModelState.IsValid)
            {
                return BadRequest(SchoolInfo);
            }
            var schoolInfo = _mapper.Map<SchoolInfo>(SchoolInfo);
            await _schoolInfoRepo.Add(schoolInfo);
            return Ok(schoolInfo);
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpPut]
        public async Task<IActionResult> UpdateSchoolInfo([FromForm] Map_SchoolInfo schoolInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(schoolInfo);
            }
            var schoolInfos = (await _schoolInfoRepo.GetRelation()).FirstOrDefault();
            if (schoolInfos == null)
            {
                return BadRequest("No school Information added");
            }
            _mapper.Map(schoolInfo, schoolInfos);
            await _schoolInfoRepo.Update(schoolInfos);
            return Ok(schoolInfo);
        }
        [Authorize(Policy = "ManageSchoolInfo")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSchoolInfo()
        {
            var schoolInfos = await _schoolInfoRepo.GetAll();
            foreach(var schoolInfo in schoolInfos)
            {
                await _schoolInfoRepo.Delete(schoolInfo.Id);
            }
            return Ok("Done");
        }
    }
}
