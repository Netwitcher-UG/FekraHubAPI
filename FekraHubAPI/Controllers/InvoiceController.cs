using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Data;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FekraHubAPI.MapModels.Courses;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using FekraHubAPI.Controllers.CoursesControllers.UploadControllers;
using FekraHubAPI.Constract;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<Student> _studentRepository;

        private readonly IMapper _mapper;

        public InvoiceController(IRepository<Student> studentRepository,
            IRepository<Invoice> invoiceRepository,
            ILogger<InvoiceController> logger,
        IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
            _logger = logger;
        }



        [Authorize(Policy = "ManageInvoice")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices(string? search)
        {
            try
            {
                var query = await _invoiceRepository.GetRelationList(
                where: !string.IsNullOrEmpty(search) ? x => 
                x.Student.FirstName.Contains(search) ||
                x.Student.LastName.Contains(search) : null,
                include:x=>x.Include(s=>s.Student),
                orderBy:x=>x.Date,
                selector: x => new
                {
                    x.Id,
                    x.FileName,
                    x.Date,
                    student = x.Student == null ? null : new
                    {
                        x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName
                    }
                },
                asNoTracking:true
                );

                return Ok(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize(Policy = "ManageInvoice")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoicesStudent([Required] int studentId)
        {
            try
            {
                var student = await _studentRepository.DataExist(x=> x.Id == studentId);
                if (!student)
                {
                    return BadRequest("Student not found");
                }
               
                var result = await _invoiceRepository.GetRelationList(
                where: x => x.Studentid == studentId,
                include: x => x.Include(s => s.Student),
                orderBy: x => x.Date,
                selector: x => new
                {
                    x.Id,
                    x.FileName,
                    x.Date,
                    student = x.Student == null ? null : new
                    {
                        x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName
                    }
                },
                asNoTracking: true
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Policy = "ManageInvoice")]
        [HttpGet("ReturnInvoice")]
        public async Task<ActionResult<IEnumerable<Invoice>>> ReturnInvoice(int Id)
        {
            try
            {
                var query = await _invoiceRepository.GetRelationSingle(
                    where:x => x.Id == Id,
                    selector:x=>x,
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true);
                if (query == null)
                {
                    return BadRequest("file not found");
                }


                var result = Convert.ToBase64String(query.file);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageChildren")]
        [HttpGet("GetInvoicesStudentForPerant")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoicesStudentForPerant([Required] int studentId)
        {
            try
            {
                var student = await _studentRepository.GetById(studentId);
                if (student == null)
                {
                    return BadRequest("Student not found");
                }
                var userId = _invoiceRepository.GetUserIDFromToken(User);
                if (userId != student.ParentID)
                {
                    return NotFound("This is not your child's information.");
                }
                var result = await _invoiceRepository.GetRelationList(
                where: x => x.Studentid == studentId,
                include: x => x.Include(s => s.Student),
                orderBy: x => x.Date,
                selector: x => new
                {
                    x.Id,
                    x.FileName,
                    x.Date,
                    student = x.Student == null ? null : new
                    {
                        x.Student.Id,
                        x.Student.FirstName,
                        x.Student.LastName
                    }
                },
                asNoTracking: true
                );
                if (!result.Any())
                {
                    return NotFound("No invoices found");
                }
               

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Policy = "ManageChildren")]
        [HttpGet("ReturnInvoiceForPerant")]
        public async Task<ActionResult<IEnumerable<Invoice>>> ReturnInvoiceForPerant(int Id)
        {
            try
            {
                var userId = _invoiceRepository.GetUserIDFromToken(User);
                var query = await _invoiceRepository.GetRelationSingle(
                    where: x => x.Id == Id && x.Student.ParentID == userId,
                    selector:x=>x,
                    returnType:QueryReturnType.SingleOrDefault,
                    asNoTracking:true);
                if (query == null)
                {
                    return BadRequest("file not found");
                }

                var result = Convert.ToBase64String(query.file);

                return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageInvoice")]
        [HttpPost]
        public async Task<IActionResult> UploadInvoice([FromForm] int studentId, IFormFile invoiceFile)
        {
            try
            {
                var student = await _studentRepository.GetById(studentId);
                if (student == null)
                {
                    return NotFound("student not found.");
                }

                if (invoiceFile.Length > 0)
                {

                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await invoiceFile.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }

                    var upload = new Invoice
                    {
                        file = fileBytes,
                        Date = DateTime.Now,
                        FileName = invoiceFile.FileName,
                        Studentid = studentId
                    };

                    await _invoiceRepository.Add(upload);
                }

                return Ok("invoice File uploaded successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageInvoice")]
        [HttpPut("{invoiceId}")]
        public async Task<IActionResult> EditUploadInvoice( int invoiceId, [FromForm] int studentId, IFormFile invoiceFile)
        {
            try
            {
                var student = await _studentRepository.DataExist(x=>x.Id == studentId);
                if (!student)
                {
                    return NotFound("student not found.");
                }

                var invoiceEntity = await _invoiceRepository.GetById(invoiceId);

                if (invoiceFile.Length > 0)
                {

                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await invoiceFile.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }

                    invoiceEntity.file = fileBytes;
                    invoiceEntity.Date = DateTime.Now;
                    invoiceEntity.FileName = invoiceFile.FileName;
                    invoiceEntity.Studentid = studentId;

                    await _invoiceRepository.Update(invoiceEntity);
                }

                return Ok("invoice File edited successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "ManageInvoice")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceFile(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.DataExist(x=> x.Id == id);
                if (!invoice)
                {
                    return NotFound();
                }

                
                await _invoiceRepository.Delete(id);

                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "InvoiceController", ex.Message));
                return BadRequest(ex.Message);
            }
        }



    }
}
