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
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Controllers.CoursesControllers.UploadControllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class UploadController : ControllerBase
    {

        
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Upload> _uploadRepository;
        private readonly IRepository<Student> _studentRepository;

        private readonly IRepository<UploadType> _uploadTypeRepository;
        private readonly IMapper _mapper;
        public UploadController(IRepository<Course> courseRepository, IRepository<Upload> uploadRepository,
            IRepository<Student> studentRepository,
            IRepository<UploadType> uploadTypeRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _uploadRepository = uploadRepository;
            _studentRepository = studentRepository;

            _uploadTypeRepository = uploadTypeRepository;
            _mapper = mapper;
         
        }



        [Authorize(Policy = "ManageFile")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Upload>>> GetUpload(string? search)
        {
            try
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
                    Courses = x.Courses == null ? null : x.Courses.Select(z => new
                    {
                        z.Id,
                        z.Name,

                    }),
                    x.FileName,
                    x.Date
                }).ToList();

                return Ok(result);
             
                }
                catch (Exception ex)
                {
                return BadRequest(ex.Message);
                }

        }
        [Authorize(Policy = "ManageFile")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Upload>>> DownloadUploadFile(int Id)
        {

            try
            {
                var query = await _uploadRepository.GetById(Id);
                if (query == null)
                {
                    return BadRequest("file not found");
                }
                var result = Convert.ToBase64String(query.file);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

        [Authorize(Policy = "ManageChildren")]
        [HttpGet("GetUploadForPerant")]
        public async Task<ActionResult<IEnumerable<Upload>>> GetUploadForPerant(string? search , [Required] int studentID)
        {

            try
            {
                var student = await _studentRepository.GetById(studentID);
                if (student == null)
                {
                    return BadRequest("Student not found");
                }

                var userId = _uploadRepository.GetUserIDFromToken(User);
                if (userId != student.ParentID)
                {
                    return NotFound("This is not your child's information.");
                }

                var courseID = student.CourseID;
                IQueryable<Upload> query = (await _uploadRepository.GetRelation()).Where(x => x.Courses.Any(z => z.Id == courseID));


                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x => x.Courses.Any(z => z.Name.Contains(search)));
                }


                var result = query.Select(x => new
                {
                    x.Id,
                    TypeUPload = x.UploadType.TypeTitle,
                    Courses = x.Courses == null ? null : x.Courses.Select(z => new
                    {
                        z.Id,
                        z.Name,

                    }),
                    x.FileName,
                    x.Date
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
          
        }
        [Authorize(Policy = "ManageChildren")]
        [HttpGet("FileForPernt")]
        public async Task<ActionResult<IEnumerable<Upload>>> FileForPerant(int Id)
        {

            try
            {
                var query = await _uploadRepository.GetById(Id);
                if (query == null)
                {
                    return BadRequest("file not found");
                }
                var result = Convert.ToBase64String(query.file);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
          
        }


        [Authorize(Policy = "ManageFile")]
        [HttpPost]
        public async Task<IActionResult> UploadFiles([FromForm] int courseId, [FromForm] int UploadTypeId, [FromForm] List<IFormFile> files)
        {

            try
            {
                return await SaveFile(courseId, files, UploadTypeId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
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
                        FileName = file.FileName,
                        Date = DateTime.Now,
                        Courses = new List<Course>()
                    };
            
                    upload.Courses.Add(course);

                    await _uploadRepository.Add(upload);

                }
            }

            return Ok("Files uploaded successfully.");
        }



        [Authorize(Policy = "DeleteFile")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUploadFile(int id)
        {

            try
            {
                var upload = await _uploadRepository.GetById(id);
                if (upload == null)
                {
                    return NotFound();
                }


                await _uploadRepository.Delete(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
          
        }



    }
}
