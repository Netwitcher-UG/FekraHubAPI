using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.Controllers.AuthorizationController;
using FekraHubAPI.Data.Models;
using FekraHubAPI.MapModels.Courses;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FekraHubAPI.Controllers.CoursesControllers.EventControllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypeController : ControllerBase
    {
        private readonly IRepository<EventType> _eventTypeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EventTypeController> _logger;
        public EventTypeController(IRepository<EventType> eventTypeRepository, IMapper mapper,
            ILogger<EventTypeController> logger)
        {
            _eventTypeRepository = eventTypeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize(Policy = "ManageEventTypes")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Map_EventType>>> GetEventTypes()
        {
            try
            {
                var eventType = await _eventTypeRepository.GetRelationList(
                    selector: x => new
                    {
                        x.Id,
                        x.TypeTitle
                    },
                    asNoTracking:true
                    );

                return Ok(eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }


        // GET: api/EventType/5
        [Authorize(Policy = "ManageEventTypes")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Map_EventType>> GetEventType(int id)
        {
            try
            {
                var eventType = await _eventTypeRepository.GetById(id);
                if (eventType == null)
                {
                    return BadRequest("Event type not found");
                }
                return Ok(new { eventType.Id, eventType.TypeTitle });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }


        // PUT: api/EventType/5
        [Authorize(Policy = "ManageEventTypes")]
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
                    return BadRequest("Event type not found");
                }

                _mapper.Map(eventTypeMdl, eventTypeEntity);
                await _eventTypeRepository.Update(eventTypeEntity);

                return Ok(new { eventTypeEntity.Id, eventTypeEntity.TypeTitle });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        [Authorize(Policy = "ManageEventTypes")]
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
                return Ok(new { eventTypeEntity.Id, eventTypeEntity.TypeTitle });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }
            

        }


        [Authorize(Policy = "ManageEventTypes")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            try
            {
                var eventType = await _eventTypeRepository.DataExist(x => x.Id == id);
                if (!eventType)
                {
                    return BadRequest("Event type not found");
                }

                await _eventTypeRepository.Delete(id);

                return Ok("Delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "EventTypeController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
    }
}
