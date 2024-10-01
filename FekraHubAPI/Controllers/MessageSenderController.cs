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
using System.Collections.Generic;

namespace FekraHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageSenderController : ControllerBase
    {
        private readonly IRepository<MessageSender> _messageSenderRepo;
        private readonly IRepository<ExternalEmails> _externalEmailRepo;
        private readonly IRepository<ApplicationUser> _UserRepo;
        private readonly IRepository<SchoolInfo> _schoolInfoRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly ILogger<MessageSenderController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MessageSenderController(IRepository<MessageSender> messageSenderRepo,
            IRepository<ApplicationUser> userRepo, IRepository<SchoolInfo> schoolInfoRepo, ILogger<MessageSenderController> logger,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager ,
            IRepository<Student> studentRepo, IRepository<ExternalEmails> externalEmailRepo)
        {
            _messageSenderRepo = messageSenderRepo;
            _UserRepo = userRepo;
            _schoolInfoRepo = schoolInfoRepo;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _studentRepo = studentRepo;
            _externalEmailRepo = externalEmailRepo;
        }

        [Authorize(Policy = "MessageSender")]
        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] PaginationParameters paginationParameters)
        {
            try
            {
                var Allmessages = await _messageSenderRepo.GetRelationAsQueryable(
                include: x => x.Include(z => z.UserMessages).ThenInclude(z => z.User)
                .Include(z => z.MessageSenderExternalEmails).ThenInclude(z => z.ExternalEmail),
                selector: x => new
                {
                    x.Id,
                    x.Message,
                    x.Date,
                    User = x.UserMessages.Select(z => new
                    {
                        z.User.Id,
                        z.User.FirstName,
                        z.User.LastName,
                        z.User.Email
                    }),
                    ExternalEmails = x.MessageSenderExternalEmails.Select(z => new
                    {
                        z.ExternalEmailId,
                        z.ExternalEmail.FirstName,
                        z.ExternalEmail.LastName,
                        z.ExternalEmail.Email
                    })

                },
                asNoTracking: true
                );
                var messages = await _messageSenderRepo.GetPagedDataAsync(Allmessages, paginationParameters);
                return Ok(new { messages.TotalCount, messages.PageSize, messages.TotalPages, messages.CurrentPage, Messages = messages.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "MessageSenderController", ex.Message));
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Policy = "MessageSender")]
        [HttpGet("UserMessages")]
        public async Task<IActionResult> GetUserMessages(string Email)
        {
            try
            {
                var ISUserds = await _messageSenderRepo.DataExist(x => x.UserMessages.Select(z => z.User.Email).Contains(Email));
                var ISExternal = await _externalEmailRepo.DataExist(m=> m.Email == Email);

                if (ISUserds)
                {
                    var messages = await _messageSenderRepo.GetRelationList(
                        include:x=> x.Include(z=>z.UserMessages).ThenInclude(z=>z.User),
                    where: x => x.UserMessages.Select(z => z.User.Email).Contains(Email),
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
                else if (ISExternal)
                {
                    var messages = await _messageSenderRepo.GetRelationList(
                        include: x => x.Include(z => z.MessageSenderExternalEmails).ThenInclude(z => z.ExternalEmail),
                    where: x => x.MessageSenderExternalEmails.Select(z => z.ExternalEmail.Email).Contains(Email),
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
                return Ok(new List<MessageSender>() { });
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "MessageSenderController", ex.Message));
                return BadRequest(ex.Message);
            }
            
        }
        //public class ExternalEmailsDTO
        //{
        //    public int? Id { get; set; }
        //    public string? FirstName { get; set; }
        //    public string? LastName { get; set; }
        //    public string Email { get; set; }
        //}
        public class MessagDTO
        {
            public string? Subject { get; set; }
            public string Message { get; set; }
            //public List<ExternalEmailsDTO>? ExternalEmails { get; set; }
            public List<string>? Emails { get; set; }
            public List<IFormFile>? Files { get; set; }
            public string? Role { get; set; }
            public int? CourseId { get; set; }

        }
        [Authorize(Policy = "MessageSender")]
        [HttpPost("UserMessages")]
        public async Task<IActionResult> GetUserMessages([FromForm] MessagDTO messagDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if ((messagDTO.Emails == null || !messagDTO.Emails.Any()) && messagDTO.Role == null && messagDTO.CourseId == null )
                {
                    return BadRequest("There are no emails to send notifications to.");
                }
                List<string> finaleEmails = new List<string>();
                List<ApplicationUser> allUsers = new List<ApplicationUser>();
                List<ExternalEmails> externalEmails = new List<ExternalEmails>();
                if (messagDTO.Emails != null && messagDTO.Emails.Any())
                {
                    HashSet<string> uniqueEmails = new HashSet<string>(messagDTO.Emails);
                    messagDTO.Emails = uniqueEmails.ToList();
                    if (messagDTO.Emails != null && messagDTO.Emails.Any())
                    {
                        foreach (var email in messagDTO.Emails)
                        {
                            var userOrNot = await _userManager.FindByEmailAsync(email);
                            if (userOrNot != null)
                            {
                                allUsers.Add(userOrNot);
                                finaleEmails.Add(email);
                            }
                            else
                            {
                                externalEmails.Add(new ExternalEmails
                                {
                                    Email = email,
                                });
                                finaleEmails.Add(email);
                            }
                        }
                        //var users = await _UserRepo.GetRelationList(
                        //where: x => messagDTO.UserId.Contains(x.Id),
                        //include: x => x.Include(u => u.UserMessages),
                        //selector: x => x
                        //);
                        //if (users == null || !users.Any())
                        //{
                        //    return BadRequest("User not found");
                        //}
                        //else
                        //{
                        //    allUsers.AddRange(users);
                        //    users.ForEach(x => Emails.Add(x.Email));
                        //}

                    }
                }
                
                
                
                if (messagDTO.Role != null)
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
                        usersInRole.ForEach(x => finaleEmails.Add(x.Email));
                    }
                }
                if (messagDTO.CourseId != null)
                {
                    var Parents = await _studentRepo.GetRelationList(
                        where: x => x.CourseID == messagDTO.CourseId,
                        include: x => x.Include(z => z.User),
                        selector: x => x.User,
                        asNoTracking: true
                        );
                    if (Parents == null || !Parents.Any())
                    {
                        return BadRequest("User not found");
                    }
                    else
                    {
                        allUsers.AddRange(Parents);
                        Parents.ForEach(x => finaleEmails.Add(x.Email));
                    }
                }

                
                
                List<ApplicationUser> uniqueUsers = allUsers
                .GroupBy(user => user.Email)
                .Select(group => group.First())
                .ToList();



                var schoolInfo = await _schoolInfoRepo.GetRelationSingle(
                    selector: x => new
                    {
                        x.SchoolName,
                        x.EmailServer,
                        x.EmailPortNumber,
                        x.FromEmail,
                        x.Password
                    },
                    asNoTracking: true,
                    returnType: QueryReturnType.Single
                    );


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(schoolInfo.SchoolName, schoolInfo.FromEmail));
                foreach (var Email in finaleEmails)
                {
                    message.Bcc.Add(new MailboxAddress("", Email));
                }
                //if (messagDTO.ExternalEmails != null && messagDTO.ExternalEmails.Any())
                //{
                //    foreach (var email in messagDTO.ExternalEmails.Select(x=>x.Email))
                //    {
                //        message.Bcc.Add(new MailboxAddress("", email));
                //    }
                //}

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
                        var contentType = new ContentType("application", file.ContentType);
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

                if (externalEmails.Any())
                {
                    var externalEmailsList = new List<ExternalEmails>();
                    foreach (var emailDTO in externalEmails)
                    {
                        var externalEmailExist = await _externalEmailRepo.DataExist(x => x.Email == emailDTO.Email);
                        ExternalEmails? externalEmail;

                        if (externalEmailExist)
                        {
                            externalEmail = await _externalEmailRepo.GetRelationSingle(
                                where: x => x.Email == emailDTO.Email,
                                selector: x => x,
                                returnType: QueryReturnType.FirstOrDefault
                            );
                           
                        }
                        else
                        {
                            externalEmail = new ExternalEmails
                            {
                                FirstName = emailDTO.FirstName,
                                LastName = emailDTO.LastName,
                                Email = emailDTO.Email
                            };
                            await _externalEmailRepo.Add(externalEmail);
                        }

                        externalEmailsList.Add(externalEmail);
                    }

                    messageSender.MessageSenderExternalEmails = externalEmailsList.Select(x => new MessageSenderExternalEmail
                    {
                        ExternalEmailId = x.Id
                    }).ToList();
                }

                await _messageSenderRepo.Add(messageSender);


                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "MessageSenderController", ex.Message));
                return BadRequest(ex.Message);
            }
           
        }


    }
}
