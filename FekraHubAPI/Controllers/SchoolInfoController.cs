using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels;
using FekraHubAPI.Repositories.Interfaces;
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

        [HttpGet]
        public async Task<IActionResult> GetSchoolInfo()
        {
            var schoolInfo = (await _schoolInfoRepo.GetAll()).FirstOrDefault();
            return Ok(schoolInfo);
        }
        
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
            var schoolInfo = _mapper.Map<SchoolInfo>(schoolInfos);
            await _schoolInfoRepo.Add(schoolInfo);
            return Ok(schoolInfo);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateSchoolInfo([FromForm] Map_SchoolInfo SchoolInfo)
        {
            var schoolInfos = await _schoolInfoRepo.GetAll();
            if (!schoolInfos.Any())
            {
                return BadRequest("No school Information has been added");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(SchoolInfo);
            }
            var schoolInfo = _mapper.Map<SchoolInfo>(schoolInfos);
            await _schoolInfoRepo.Update(schoolInfo);
            return Ok(schoolInfo);
        }
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
