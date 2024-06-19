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

namespace FekraHubAPI.Controllers.CoursesControllers.UploadControllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class UploadController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Upload> _uploadRepository;
        private readonly IRepository<UploadCourse> _courseUploadRepository;
        private readonly IRepository<UploadType> _uploadTypeRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        public UploadController(IRepository<Course> courseRepository, IRepository<Upload> uploadRepository,
            IRepository<UploadCourse> courseUploadRepository,
            ApplicationDbContext context,
            IRepository<UploadType> uploadTypeRepository, IMapper mapper, IWebHostEnvironment env)
        {
            _courseRepository = courseRepository;
            _uploadRepository = uploadRepository;
            _courseUploadRepository = courseUploadRepository;
            _uploadTypeRepository = uploadTypeRepository;
            _mapper = mapper;
            _env = env;
            _context = context;
        }
      


        // GET: api/UploadTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Upload>>> GetUploadTypes()
        {

            IQueryable<Upload> query = (await _uploadRepository.GetRelation());
            var result = query.Select(x => new
            {
                   x.Id,
                   x.file,
                   TypeUPload= x.UploadType.TypeTitle
           
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
                        file = fileBytes

                    };

                   

                    await _uploadRepository.Add(upload);


                    var UploadCourse = new Map_UploadCourse
                    {
                        CourseID = courseId,
                        UploadID = upload.Id

                    };
                    var UploadCourseEntity = _mapper.Map<UploadCourse>(UploadCourse);

                    await _courseUploadRepository.Add(UploadCourseEntity);

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

            var UploadsCourse = await _context.UploadsCourse.SingleOrDefaultAsync(ut => ut.UploadID == id);


            await _courseUploadRepository.Delete(UploadsCourse.Id);
            
            await _uploadRepository.Delete(id);

            return NoContent();
        }



    }
}
