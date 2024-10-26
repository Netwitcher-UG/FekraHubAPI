using FekraHubAPI.Constract;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using System.ComponentModel.DataAnnotations;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IRepository<Notifications> _notificationsRepo;
        private readonly IRepository<NotificationUser> _notificationUserRepo;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NotificationsController> _logger;
        public NotificationsController(IRepository<Notifications> notificationsRepo, IRepository<NotificationUser> notificationUserRepo,
            ILogger<NotificationsController> logger,ApplicationDbContext db)
        {
            _notificationsRepo = notificationsRepo;
            _notificationUserRepo = notificationUserRepo;
            _logger = logger;
            _db = db;   
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var userId = _notificationsRepo.GetUserIDFromToken(User);
                var userNotifications = await _notificationUserRepo.GetRelationList(
                     where:x=>x.UserId == userId,
                     selector:x => new
                     {
                         x.Notifications.Id,
                         x.Notifications.Notification,
                         x.Notifications.Date,
                         x.Read
                     },
                     asNoTracking:true,
                     orderBy:x=>x.NotificationId
                    );
                var unReadCount = userNotifications.Where(x => x.Read == false).Count();
                return Ok(new { userNotifications , unReadCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "NotificationsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPatch("NotificationsRead")]
        public async Task<IActionResult> UpdateNotifications(int? id = null,bool AllRead = false)
        {
            try
            {
                if (!id.HasValue && AllRead == false)
                {
                    return BadRequest();
                }
                var userId = _notificationsRepo.GetUserIDFromToken(User);
                if (id.HasValue)
                {
                    
                    var Notifications = await _notificationUserRepo.GetRelationSingle(
                        where: x => id == x.NotificationId && x.UserId == userId,
                        selector: x => x,
                        returnType: QueryReturnType.FirstOrDefault
                        );
                    if (Notifications == null)
                    {
                        return BadRequest("Benachrichtigung nicht gefunden.");//notification not found
                    }
                    Notifications.Read = true;
                    await _notificationUserRepo.Update(Notifications);
                }
                if (AllRead)
                {
                    var AllNotifications = await _notificationUserRepo.GetRelationList(
                        where: x => x.UserId == userId,
                        selector: x => x
                       
                        );
                    if (AllNotifications == null)
                    {
                        return BadRequest("Benachrichtigung nicht gefunden.");//notification not found
                    }
                    AllNotifications.ForEach(x=> x.Read = true);
                    await _notificationUserRepo.ManyUpdate(AllNotifications);
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "NotificationsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteNotifications([Required] List<int> ids)
        {
            try
            {
                var userId = _notificationsRepo.GetUserIDFromToken(User);
                var notificationsToDelete = await _db.NotificationUser
                .Where(nu => ids.Contains(nu.NotificationId) && nu.UserId == userId)
                .ToListAsync();
                if (notificationsToDelete == null || !notificationsToDelete.Any())
                {
                    return BadRequest("Benachrichtigung nicht gefunden.");//notification not found
                }

                foreach (var notificationUser in notificationsToDelete)
                {
                    _db.NotificationUser.Remove(notificationUser);
                    var otherUsersCount = await _db.NotificationUser
                        .Where(nu => nu.NotificationId == notificationUser.NotificationId && nu.UserId != userId)
                        .CountAsync();

                    if (otherUsersCount == 0)
                    {
                        var notification = await _db.Notifications
                            .SingleOrDefaultAsync(n => n.Id == notificationUser.NotificationId);

                        if (notification != null)
                        {
                            _db.Notifications.Remove(notification);
                        }
                    }

                    
                }

                await _db.SaveChangesAsync();
                

                return Ok("Erfolgreich gelöscht.");//deleted success
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "NotificationsController", ex.Message));
                return BadRequest(ex.Message);
            }
        }






    }
}
