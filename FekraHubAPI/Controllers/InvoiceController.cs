using AutoMapper;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Data;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FekraHubAPI.MapModels.Courses;
using System;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {


      
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<Student> _studentRepository;
      
        private readonly IMapper _mapper;
   
        public InvoiceController(IRepository<Student> studentRepository,
            IRepository<Invoice> invoiceRepository,
              
        IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        
        }



        [Authorize(Policy = "ManageInvoice")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices(string? search)
        {
            IQueryable<Invoice> query = (await _invoiceRepository.GetRelation());

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Student.FirstName.Contains(search) || x.Student.LastName.Contains(search));
              
            }


            var result = query.Select(x => new
            {
                x.Id,
                x.FileName,
                x.Date,
                x.Student.FirstName,
                x.Student.LastName

            }).ToList();

            return Ok(result);
        }
        [Authorize(Policy = "ManageChildren")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoicesStudent(int studentId)
        {
            IQueryable<Invoice> query = (await _invoiceRepository.GetRelation());
            var student = await _studentRepository.GetById(studentId);
            var userId = _invoiceRepository.GetUserIDFromToken(User);
            if (userId != student.ParentID)
            {
                return NotFound("This is not your child's information.");
            }

            query = query.Where(x => x.Studentid == studentId);


            var result = query.Select(x => new
            {
                x.Id,
                x.FileName,
                x.Date,
                x.Student.FirstName,
                x.Student.LastName

            }).ToList();

            return Ok(result);
        }

        [Authorize(Policy = "ManageChildren")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Invoice>>> ReturnInvoice(int Id)
        {
            var query = await _invoiceRepository.GetById(Id);
            if (query == null)
            {
                return BadRequest("file not found");
            }

            var userId = _invoiceRepository.GetUserIDFromToken(User);
            if (userId != query.Student.ParentID)
            {
                return NotFound("This is not your child's information.");
            }

            var result = Convert.ToBase64String(query.file);

            return Ok(result);
        }

        [Authorize(Policy = "ManageInvoice")]
        [HttpPost]
        public async Task<IActionResult> UploadInvoice([FromForm] int studentId, IFormFile invoiceFile)
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


       



        [Authorize(Policy = "ManageInvoice")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceFile(int id)
        {

            var upload = await _invoiceRepository.GetById(id);
            if (upload == null)
            {
                return NotFound();
            }


            await _invoiceRepository.Delete(id);

            return NoContent();
        }



    }
}
