using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using FekraHubAPI.Constract;
using FekraHubAPI.Migrations;
using Microsoft.AspNetCore.Identity;
using FekraHubAPI.MapModels;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageSenderController : ControllerBase
    {
        private readonly IRepository<MessageSender> _messageSenderRepo;
        private readonly IRepository<ApplicationUser> _UserRepo;
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly ILogger<PayRollsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MessageSenderController(IRepository<MessageSender> messageSenderRepo,
            IRepository<ApplicationUser> userRepo, IRepository<SchoolInfo> schoolInfoRepo, ILogger<PayRollsController> logger,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager , IRepository<Student> studentRepo)
        {
            _messageSenderRepo = messageSenderRepo;
            _UserRepo = userRepo;
            _schoolInfoRepo = schoolInfoRepo;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _studentRepo = studentRepo;
        }

        [Authorize(Policy = "MessageSender")]
        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] PaginationParameters paginationParameters)
        {
            var Allmessages = await _messageSenderRepo.GetRelationAsQueryable(
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
            var messages = await _messageSenderRepo.GetPagedDataAsync(Allmessages, paginationParameters);
            return Ok(new { messages.TotalCount, messages.PageSize, messages.TotalPages, messages.CurrentPage, Messages = messages.Data });
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
            public List<string>? ExternalEmails { get; set; }
            public List<string>? UserId { get; set; }
            public List<IFormFile>? Files { get; set; }
            public string? Role { get; set; }
            public int? CourseId { get; set; }

        }
        [Authorize(Policy = "MessageSender")]
        [HttpPost("UserMessages")]
        public async Task<IActionResult> GetUserMessages([FromForm] MessagDTO messagDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if((messagDTO.UserId == null || !messagDTO.UserId.Any()) && messagDTO.Role == null && messagDTO .CourseId == null)
            {
                return BadRequest("There are no users to send notifications to.");
            }
            List<string> Emails = new List<string>();
            List<ApplicationUser> allUsers = new List<ApplicationUser>();
            if(messagDTO.UserId != null && messagDTO.UserId.Any())
            {
                var users = await _UserRepo.GetRelationList(
                where: x => messagDTO.UserId.Contains(x.Id),
                include: x => x.Include(u => u.UserMessages),
                selector: x => x
                );
                if (users == null || !users.Any())
                {
                    return BadRequest("User not found");
                }
                else
                {
                    allUsers.AddRange(users);
                    users.ForEach(x => Emails.Add(x.Email));
                }
                
            }
            if(messagDTO.Role != null)
            {
                var role = await _roleManager.FindByNameAsync(messagDTO.Role);
                if (role == null)
                {
                    return NotFound(new { Message = "Role not found" });
                }

                var usersInRole = (await _userManager.GetUsersInRoleAsync(messagDTO.Role)).ToList();
                if (usersInRole == null || !usersInRole.Any())
                {
                    return BadRequest("User not found");
                }
                else
                {
                    allUsers.AddRange(usersInRole);
                    usersInRole.ForEach(x => Emails.Add(x.Email));
                }
            }
            if (messagDTO.CourseId != null)
            {
                var Parents = await _studentRepo.GetRelationList(
                    where:x=>x.CourseID == messagDTO.CourseId,
                    include:x=>x.Include(z=>z.User),
                    selector:x=>x.User,
                    asNoTracking:true
                    );
                if (Parents == null || !Parents.Any())
                {
                    return BadRequest("User not found");
                }
                else
                {
                    allUsers.AddRange(Parents);
                    Parents.ForEach(x => Emails.Add(x.Email));
                }
            }

            HashSet<string> uniqueEmails = new HashSet<string>(Emails);
            Emails = uniqueEmails.ToList();
            List<ApplicationUser> uniqueUsers = allUsers
            .GroupBy(user => user.Email)
            .Select(group => group.First())
            .ToList();

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
            foreach (var Email in Emails)
            {
                message.Bcc.Add(new MailboxAddress("", Email));
            }
            if (messagDTO.ExternalEmails != null && messagDTO.ExternalEmails.Any())
            {
                foreach(var email in messagDTO.ExternalEmails)
                {
                    message.Bcc.Add(new MailboxAddress("", email));
                }
            }

            message.Subject = messagDTO.Subject;

            var bodyBuilder = new BodyBuilder
            {
                
                HtmlBody = messagDTO.Message
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
                UserMessages = uniqueUsers.Select(user => new UserMessage
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
                User = uniqueUsers.Select(x=>new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.Email
                })
            });
        }


    }
}
