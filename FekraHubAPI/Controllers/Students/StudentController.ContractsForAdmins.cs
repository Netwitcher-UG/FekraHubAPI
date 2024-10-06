using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Controllers.Students
{
    public partial class StudentController
    {

        [Authorize(Policy = "GetContracts")]
        [HttpGet("Contracts")]
        public async Task<IActionResult> GetContracts()
        {
            try
            {
                var contracts = await _studentContractRepo.GetRelationList(
                    orderBy: x => x.Id,
                    include: x=> x.Include(s=>s.Student).ThenInclude(u=>u.User),
                    selector: x => new {
                        x.Id,
                        x.StudentID,
                        x.Student.FirstName,
                        x.Student.LastName,
                        ParentId = x.Student.ParentID,
                        ParentFirstName = x.Student.User.FirstName,
                        ParentLastName = x.Student.User.LastName,
                        x.CreationDate,

                    },
                    asNoTracking:true
                    );
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "GetContracts")]
        [HttpGet("GetContractsByStudent")]
        public async Task<IActionResult> GetContractsByStudent([Required] int studentId)
        {
            try
            {
                
                var contracts = await _studentContractRepo.GetRelationList(
                    where: x => x.StudentID == studentId,
                    orderBy: x => x.Id,
                    include: x => x.Include(s => s.Student).ThenInclude(u => u.User),
                    selector: x => new {
                        x.Id,
                        x.StudentID,
                        x.Student.FirstName,
                        x.Student.LastName,
                        ParentId = x.Student.ParentID,
                        ParentFirstName = x.Student.User.FirstName,
                        ParentLastName = x.Student.User.LastName,
                        x.CreationDate,

                    },
                    asNoTracking: true
                    );
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "GetContracts")]
        [HttpGet("DownloadContractFileForAdmin")]
        public async Task<ActionResult<IEnumerable<Upload>>> DownloadContractFileForAdmin(int contractId)
        {
            try
            {
                var query = await _studentContractRepo.GetById(contractId);
                if (query == null)
                {
                    return BadRequest("Datei nicht gefunden.");//file not found
                }
                var result = Convert.ToBase64String(query.File);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

    }
}
