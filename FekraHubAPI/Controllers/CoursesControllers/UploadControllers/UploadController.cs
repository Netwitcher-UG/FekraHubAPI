using AutoMapper;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Authorization;

namespace FekraHubAPI.Controllers.CoursesControllers.UploadControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]


    public class UploadController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Upload> _uploadRepository;

        private readonly IRepository<UploadType> _uploadTypeRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        public UploadController(IRepository<Course> courseRepository, IRepository<Upload> uploadRepository,

            ApplicationDbContext context,
            IRepository<UploadType> uploadTypeRepository, IMapper mapper, IWebHostEnvironment env)
        {
            _courseRepository = courseRepository;
            _uploadRepository = uploadRepository;

            _uploadTypeRepository = uploadTypeRepository;
            _mapper = mapper;
            _env = env;
            _context = context;
        }



        // GET: api/UploadTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Upload>>> GetUploadTypes(string? search)
        {

           

            IQueryable<Upload> query = (await _uploadRepository.GetRelation());

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Courses.Any(z => z.Name.Contains(search)));
            }


            var result = query.Select(x => new
            {
                x.Id,
                TypeUPload = x.UploadType.TypeTitle,
                Courses = x.Courses.Select(z => new
                {
                    z.Id,
                    z.Name,

                }),
                x.file,

            }).ToList();

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> UploadFiles([FromForm] int courseId, [FromForm] int UploadTypeId, [FromForm] List<IFormFile> files)
        {

            return await SaveFile(courseId, files, UploadTypeId);
        }


        private async Task<IActionResult> SaveFile(int courseId, List<IFormFile> files, int TypeId)
        {
            var course = await _courseRepository.GetById(courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }


            foreach (var file in files)
            {
                if (file.Length > 0)
                {

                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }

                    var upload = new Upload
                    {
                        UploadTypeid = TypeId,
                        file = fileBytes,
                        FileName = file.Name,
                        Courses = new List<Course>()
                    };
                    var existingCourse = await _courseRepository.GetById(courseId);
                    if (existingCourse == null)
                    {
                        return NotFound("Course not found.");
                    }
                    upload.Courses.Add(existingCourse);

                    await _uploadRepository.Add(upload);

                }
            }

            return Ok("Files uploaded successfully.");
        }



        // DELETE: api/UploadTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUploadFile(int id)
        {

            var upload = await _uploadRepository.GetById(id);
            if (upload == null)
            {
                return NotFound();
            }


            await _uploadRepository.Delete(id);

            return NoContent();
        }



    }
}
