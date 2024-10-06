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
using System.Linq.Expressions;
using FekraHubAPI.Controllers.CoursesControllers.EventControllers;
using FekraHubAPI.Constract;
using FekraHubAPI.EmailSender;

namespace FekraHubAPI.Controllers.CoursesControllers.UploadControllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class UploadController : ControllerBase
    {

        
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Upload> _uploadRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly ILogger<UploadController> _logger;
        private readonly IRepository<UploadType> _uploadTypeRepository;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        public UploadController(IRepository<Course> courseRepository, IRepository<Upload> uploadRepository,
            IRepository<Student> studentRepository,
            IRepository<UploadType> uploadTypeRepository, IMapper mapper,
            ILogger<UploadController> logger, IEmailSender emailSender)
        {
            _courseRepository = courseRepository;
            _uploadRepository = uploadRepository;
            _studentRepository = studentRepository;
            _logger = logger;
            _uploadTypeRepository = uploadTypeRepository;
            _mapper = mapper;
            _emailSender = emailSender;
        }



        [Authorize(Policy = "ManageFile")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Upload>>> GetUpload(string? search , int? studentId)
        {
            try
            {
                var result = await _uploadRepository.GetRelationList(
                    manyWhere: new List<Expression<Func<Upload, bool>>?>
                    {
                        !string.IsNullOrEmpty(search) ? (Expression<Func<Upload, bool>>)(x => x.Courses.Any(z => z.Name.Contains(search))) : null,
                        studentId != null ? (Expression<Func<Upload, bool>>)(x => x.Courses.Any(z => z.Student.Any(y => y.Id == studentId))) : null
                    }.Where(x => x != null).Cast<Expression<Func<Upload, bool>>>().ToList(),
                    selector: x => new
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
                    },
                    asNoTracking: true);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UploadController", ex.Message));
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
                    return BadRequest("Datei nicht gefunden.");//file not found
                }
                var result = Convert.ToBase64String(query.file);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UploadController", ex.Message));
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
                    return BadRequest("Schüler nicht gefunden.");//Student not found
                }

                var userId = _uploadRepository.GetUserIDFromToken(User);
                if (userId != student.ParentID)
                {
                    return BadRequest("Dies sind nicht die Informationen Ihres Kindes.");//This is not your child's information.
                }

                var courseID = student.CourseID;
                var result = await _uploadRepository.GetRelationList(
                    where: x => x.Courses.Any(z => z.Id == courseID),
                    manyWhere: new List<Expression<Func<Upload, bool>>?>
                    {
                        !string.IsNullOrEmpty(search) ? (Expression<Func<Upload, bool>>)(x => x.Courses.Any(z => z.Name.Contains(search))) : null,
                    }.Where(x => x != null).Cast<Expression<Func<Upload, bool>>>().ToList(),
                    include:x=>x.Include(c=>c.Courses),
                    selector: x => new
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
                    },
                    asNoTracking:true);

              

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UploadController", ex.Message));
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
                    return BadRequest("Datei nicht gefunden.");//file not found
                }
                var result = Convert.ToBase64String(query.file);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UploadController", ex.Message));
                return BadRequest(ex.Message);
            }
          
        }

        public class UploadFilesDTO {
            public int courseId { get; set; }
            public int UploadTypeId { get; set; }
            public List<IFormFile> files    { get; set; }
        }
        [Authorize(Policy = "ManageFile")]
        [HttpPost]
        public async Task<IActionResult> UploadFiles([FromForm] UploadFilesDTO uploadFilesDTO )
        {
        

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var course = await _courseRepository.GetById(uploadFilesDTO.courseId);
                if (course == null)
                {
                    return BadRequest("Kurs nicht gefunden.");//Course not found.
                }
                var Type = await _uploadTypeRepository.GetById(uploadFilesDTO.UploadTypeId);
                if (Type == null)
                {
                    return BadRequest("Typ nicht gefunden.");//Type not found.
                }

                if (uploadFilesDTO.files == null || !uploadFilesDTO.files.Any() )
                {
                    return BadRequest("Datei nicht gefunden.");//files not found.
                }


                foreach (var file in uploadFilesDTO.files)
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
                            UploadTypeid = uploadFilesDTO.UploadTypeId
                            ,
                            file = fileBytes,
                            FileName = file.FileName,
                            Date = DateTime.Now,
                            Courses = new List<Course>()
                        };

                        upload.Courses.Add(course);

                        await _uploadRepository.Add(upload);
                    }
                }
                await _emailSender.SendToParentsNewFiles(uploadFilesDTO.courseId);

                return Ok("Dateien erfolgreich hochgeladen.");//Files uploaded successfully.
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UploadController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


    



        [Authorize(Policy = "DeleteFile")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUploadFile(int id)
        {

            try
            {
                var upload = await _uploadRepository.DataExist(X=>X.Id == id);
                if (!upload)
                {
                    return BadRequest("Datei nicht gefunden.");//File not found
                }


                await _uploadRepository.Delete(id);

                return Ok("Erfolgreich gelöscht");//Deleted success
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "UploadController", ex.Message));
                return BadRequest(ex.Message);
            }
          
        }



    }
}
