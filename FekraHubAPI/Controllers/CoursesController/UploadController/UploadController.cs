using AutoMapper;
using FekraHubAPI.@class.Courses;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace FekraHubAPI.Controllers.CoursesController.UploadController
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

      

        [HttpPost("{courseId}/{UploadTypeId}/upload")]
        public async Task<IActionResult> UploadFiles(int courseId,int UploadTypeId, List<IFormFile> files)
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

                    var upload = new Map_Upload
                    {
                        UploadTypeID = TypeId,
                        file = fileBytes

                    };

                    var uploadEntity = _mapper.Map<Upload>(upload);

                    await _uploadRepository.Add(uploadEntity);


                    var UploadCourse = new Map_UploadCourse
                    {
                        CourseID = courseId,
                        UploadID = uploadEntity.Id

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
