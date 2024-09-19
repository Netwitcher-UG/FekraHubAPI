using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using FekraHubAPI.Constract;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageSenderController : ControllerBase
    {
        private readonly IRepository<MessageSender> _messageSenderRepo;
        private readonly IRepository<ApplicationUser> _UserRepo;
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly ILogger<PayRollsController> _logger;
        public MessageSenderController(IRepository<MessageSender> messageSenderRepo,
            IRepository<ApplicationUser> userRepo, IRepository<SchoolInfo> schoolInfoRepo, ILogger<PayRollsController> logger)
        {
            _messageSenderRepo = messageSenderRepo;
            _UserRepo = userRepo;
            _schoolInfoRepo = schoolInfoRepo;
            _logger = logger;
        }

        [Authorize(Policy = "MessageSender")]
        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _messageSenderRepo.GetRelationList(
                include:x=>x.Include(z=>z.UserMessages).ThenInclude(z=>z.User),
                selector:x=> new
                {
                    x.Id,
                    x.Message,
                    x.Date,
                    User = x.UserMessages.Select(z=> new
                    {
                        z.User.Id,
                        z.User.FirstName,
                        z.User.LastName,
                        z.User.Email
                    })

                },
                asNoTracking:true
                );
            return Ok(messages);
        }

        [Authorize(Policy = "MessageSender")]
        [HttpGet("UserMessages")]
        public async Task<IActionResult> GetUserMessages(string UserId)
        {
            var messages = await _messageSenderRepo.GetRelationList(
                where:x=>x.UserMessages.Select(z=>z.UserId).Contains(UserId),
                selector: x => new
                {
                    x.Id,
                    x.Message,
                    x.Date,
                },
                asNoTracking: true
                );
            return Ok(messages);
        }
        public class MessagDTO
        {
            public string? Subject { get; set; }
            public string Message { get; set; }
            public List<string> UserId { get; set; }
            public List<IFormFile>? Files { get; set; }

        }
        [Authorize(Policy = "MessageSender")]
        [HttpPost("UserMessages")]
        public async Task<IActionResult> GetUserMessages([FromForm] MessagDTO messagDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var users = await _UserRepo.GetRelationList(
                where: x => messagDTO.UserId.Contains(x.Id),
                include: x => x.Include(u => u.UserMessages),
                selector: x => x
                );
            if (users == null || !users.Any())
            {
                return BadRequest("User not found");
            }
            var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                selector: x => new
                {
                    x.SchoolName, x.EmailServer, x.EmailPortNumber, x.FromEmail, x.Password
                },
                asNoTracking: true,
                returnType: QueryReturnType.Single
                );
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(schoolInfo.SchoolName, schoolInfo.FromEmail));
            foreach (var user in users)
            {
                message.Bcc.Add(new MailboxAddress("", user.Email));
            }


            message.Subject = messagDTO.Subject;

            var bodyBuilder = new BodyBuilder
            {
                
                TextBody = messagDTO.Message
            };

            if (messagDTO.Files != null && messagDTO.Files.Any())
            {
                foreach (var file in messagDTO.Files)
                {
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }
                    var contentType = new ContentType("application",file.ContentType);
                    bodyBuilder.Attachments.Add(file.FileName, fileBytes, contentType);
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(schoolInfo.EmailServer, schoolInfo.EmailPortNumber, MailKit.Security.SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(schoolInfo.FromEmail, schoolInfo.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(HandleLogFile.handleErrLogFile(User, "MessageSenderController", ex.Message));
                    return BadRequest(ex.Message);
                }

            }


            var messageSender = new MessageSender
            {
                Message = messagDTO.Message,
                UserMessages = users.Select(user => new UserMessage
                {
                    UserId = user.Id
                }).ToList()
            };
            await _messageSenderRepo.Add(messageSender);


            return Ok(new
            {
                messageSender.Id,
                messageSender.Date,
                messageSender.Message,
                User = messageSender.UserMessages.Select(x=>new
                {
                    x.UserId,
                    x.User.FirstName,
                    x.User.LastName,
                    x.User.Email
                })
            });
        }


    }
}
