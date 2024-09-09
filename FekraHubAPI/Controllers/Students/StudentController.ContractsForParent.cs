using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Controllers.Students
{
    public partial class StudentController
    {

        [Authorize(Policy = "ManageChildren")]
        [HttpPost]
        public async Task<IActionResult> GetContract([FromForm] Map_Student student)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var parentId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(parentId))
                {
                    return Unauthorized("User not found.");
                }

                var studentEntity = new Student
                {
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Nationality = student.Nationality,
                    Note = student.Note,
                    Gender = student.Gender,
                    Birthday = student.Birthday,
                    City = student.City ?? "Like parent",
                    Street = student.Street ?? "Like parent",
                    StreetNr = student.StreetNr ?? "Like parent",
                    ZipCode = student.ZipCode ?? "Like parent",
                    CourseID = student.CourseID,
                    ParentID = parentId,
                };
                var contract = await _contractMaker.ContractHtml(studentEntity);
                return Ok(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageChildren")]
        [HttpPost("AcceptedContract")]
        public async Task<IActionResult> AcceptedContract([FromForm] Map_Student student)
        {
            
            try
            {
                var userId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var studentEntity = new Student
                {
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Nationality = student.Nationality,
                    Note = student.Note ?? "",
                    Gender = student.Gender,
                    Birthday = student.Birthday,
                    City = student.City,
                    Street = student.Street,
                    StreetNr = student.StreetNr,
                    ZipCode = student.ZipCode,
                    ParentID = userId,
                    CourseID = student.CourseID,
                };
                await _studentRepo.Add(studentEntity);
                await _contractMaker.ConverterHtmlToPdf(studentEntity);
                _ = Task.Run(() => _emailSender.SendContractEmail(studentEntity.Id, $"{studentEntity.FirstName}_{studentEntity.LastName}_Contract"));
                _ = Task.Run(() => _emailSender.SendToAdminNewStudent(studentEntity));
                return Ok("welcomes your son to our family . A copy of the contract was sent to your email");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageChildren")]
        [HttpGet("SonsContractsForParent")]
        public async Task<IActionResult> GetSonsOfParentContracts()
        {
            try
            {
                var userId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found.");
                }
                var contracts = await _studentContractRepo.GetRelationList(
                    where: x => x.Student.ParentID == userId,
                    include:x=>x.Include(s=>s.Student),
                    selector: x => new {
                        x.Id,
                        studentId = x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName,
                        x.CreationDate,

                    },
                    asNoTracking:true);
                
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "ManageChildren")]
        [HttpGet("SonContractsForParent")]
        public async Task<IActionResult> GetSonOfParentContracts([Required] int studentId)
        {
            try
            {
                var userId = _courseRepo.GetUserIDFromToken(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found.");
                }
                var parentId = userId;
                var contracts = await _studentContractRepo.GetRelationList(
                    where: x => x.Student.ParentID == parentId && x.StudentID == studentId,
                    include:x=>x.Include(s=>s.Student),
                    selector: x => new {
                        x.Id,
                        studentId = x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName,
                        x.CreationDate,
                    },
                    asNoTracking:true);

                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "StudentController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "ManageChildren")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Upload>>> DownloadContractFile(int contractId)
        {
            try
            {
                var query = await _studentContractRepo.GetById(contractId);
                if (query == null)
                {
                    return BadRequest("file not found");
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
