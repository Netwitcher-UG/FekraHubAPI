using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypeController : ControllerBase
    {
        private readonly IRepository<EventType> _eventTypeRepository;
        private readonly IMapper _mapper;
        
        private readonly ILogger<EventTypeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public EventTypeController(IRepository<EventType> eventTypeRepository, IMapper mapper
              , ILogger<EventTypeController> logger
            )
        {
            _eventTypeRepository = eventTypeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/EventType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_EventType>>> GetEventTypes()
        {
            try
            {
                var eventType = await _eventTypeRepository.GetAll();

                return Ok(eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


        // GET: api/EventType/5

        [HttpGet("{id}")]
        public async Task<ActionResult<Map_EventType>> GetEventType(int id)
        {
            try
            {
                var eventType = await _eventTypeRepository.GetById(id);
                if (eventType == null)
                {
                    return NotFound();
                }
                return Ok(eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }

        }


        // PUT: api/EventType/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEventType(int id, [FromForm] Map_EventType eventTypeMdl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var eventTypeEntity = await _eventTypeRepository.GetById(id);
                if (eventTypeEntity == null)
                {
                    return NotFound();
                }

                _mapper.Map(eventTypeMdl, eventTypeEntity);
                await _eventTypeRepository.Update(eventTypeEntity);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        // POST: api/EventType
        [HttpPost]
        public async Task<ActionResult<EventType>> PostEventType([FromForm] Map_EventType eventType)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var eventTypeEntity = _mapper.Map<EventType>(eventType);
                await _eventTypeRepository.Add(eventTypeEntity);
                return CreatedAtAction("GetEventType", new { id = eventTypeEntity.Id }, eventTypeEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }


        }


        // DELETE: api/EventType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            try
            {
                var eventType = await _eventTypeRepository.GetById(id);
                if (eventType == null)
                {
                    return NotFound();
                }

                await _eventTypeRepository.Delete(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(await GetCurrentUserAsync(), "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
    }
}
