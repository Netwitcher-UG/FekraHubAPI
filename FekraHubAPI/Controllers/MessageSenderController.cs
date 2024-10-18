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
        private readonly IRepository<Course> _courseRepo;
        private readonly ILogger<MessageSenderController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MessageSenderController(IRepository<MessageSender> messageSenderRepo,
            IRepository<ApplicationUser> userRepo, IRepository<SchoolInfo> schoolInfoRepo, ILogger<MessageSenderController> logger,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IRepository<Student> studentRepo, IRepository<ExternalEmails> externalEmailRepo, IRepository<Course> courseRepo)
        {
            _messageSenderRepo = messageSenderRepo;
            _UserRepo = userRepo;
            _schoolInfoRepo = schoolInfoRepo;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _studentRepo = studentRepo;
            _externalEmailRepo = externalEmailRepo;
            _courseRepo = courseRepo;
        }

        [Authorize(Policy = "MessageSender")]
        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            try
            {
                var Messages = await _messageSenderRepo.GetRelationAsQueryable(
                include: x => x.Include(z => z.UserMessages).ThenInclude(z => z.User)
                .Include(z => z.MessageSenderExternalEmails).ThenInclude(z => z.ExternalEmail),
                selector: x => new
                {
                    x.Id,
                    x.Subject,
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
                asNoTracking: true,
                orderBy:x=>x.Date
                );

                return Ok(Messages);
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
                var ISExternal = await _externalEmailRepo.DataExist(m => m.Email == Email);

                if (ISUserds)
                {
                    var messages = await _messageSenderRepo.GetRelationList(
                        include: x => x.Include(z => z.UserMessages).ThenInclude(z => z.User),
                    where: x => x.UserMessages.Select(z => z.User.Email).Contains(Email),
                    selector: x => new
                    {
                        x.Id,
                        x.Subject,
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
                        x.Subject,
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
            public List<string>? Role { get; set; }
            public List<int>? CourseId { get; set; }

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
                if ((messagDTO.Emails == null || !messagDTO.Emails.Any()) && messagDTO.Role == null && messagDTO.CourseId == null)
                {
                    return BadRequest("Es gibt keine E-Mails, an die Benachrichtigungen gesendet werden können.");//There are no emails to send notifications to.
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



                if (messagDTO.Role != null && messagDTO.Role.Any())
                {
                    foreach (var roleName in messagDTO.Role)
                    {
                        var role = await _roleManager.FindByNameAsync(roleName);
                        if (role == null)
                        {
                            return BadRequest($"Rolle '{roleName}' nicht gefunden");//Role '{roleName}' not found
                        }

                        var usersInRole = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
                        if (usersInRole == null || !usersInRole.Any())
                        {
                            return BadRequest($"Keine Benutzer für die Rolle '{roleName}' gefunden");//No users found for role '{roleName}'
                        }
                        else
                        {
                            allUsers.AddRange(usersInRole);
                            usersInRole.ForEach(x => finaleEmails.Add(x.Email));
                        }
                    }
                }

                if (messagDTO.CourseId != null && messagDTO.CourseId.Any())
                {
                    var teachers = await _courseRepo.GetRelationSingle(
                        where: x => messagDTO.CourseId.Contains(x.Id),
                        selector: x => x.Teacher.ToList(),
                        asNoTracking: true
                        );
                    if (teachers != null && teachers.Any())
                    {
                        allUsers.AddRange(teachers);
                        teachers.ForEach(x => finaleEmails.Add(x.Email));
                    }

                    foreach (var courseId in messagDTO.CourseId)
                    {
                        var parents = await _studentRepo.GetRelationList(
                            where: x => x.CourseID == courseId,
                            include: x => x.Include(z => z.User),
                            selector: x => x.User,
                            asNoTracking: true
                        );

                        if (parents == null || !parents.Any())
                        {
                            return BadRequest($"Keine Benutzer für die Kurs-ID {courseId} gefunden");//No users found for Course ID {courseId}
                        }
                        else
                        {
                            allUsers.AddRange(parents);
                            parents.ForEach(x => finaleEmails.Add(x.Email));
                        }
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

                message.Subject = messagDTO.Subject ?? "";

                var bodyBuilder = new BodyBuilder
                {

                    HtmlBody = MessageLayout(messagDTO.Message, schoolInfo.SchoolName)
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
                var germanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                var germanTime = TimeZoneInfo.ConvertTime(DateTime.Now, germanTimeZone);

                var messageSender = new MessageSender
                {
                    Date = germanTime,
                    Subject = messagDTO.Subject,
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


                return Ok("Erfolg");//success
            }
            catch (Exception ex)
            {
                _logger.LogError(HandleLogFile.handleErrLogFile(User, "MessageSenderController", ex.Message));
                return BadRequest(ex.Message);
            }

        }
        private string MessageLayout(string contentHtml, string schoolName)
        {

            string ConstantsMessage = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html dir=""ltr"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"">
 <head>
  <meta charset=""UTF-8"">
  <meta content=""width=device-width, initial-scale=1"" name=""viewport"">
  <meta name=""x-apple-disable-message-reformatting"">
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
  <meta content=""telephone=no"" name=""format-detection"">
  <title>confirm</title><!--[if (mso 16)]>
    <style type=""text/css"">
    a {text-decoration: none;}
    </style>
    <![endif]--><!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]--><!--[if gte mso 9]>
<xml>
    <o:OfficeDocumentSettings>
    <o:AllowPNG></o:AllowPNG>
    <o:PixelsPerInch>96</o:PixelsPerInch>
    </o:OfficeDocumentSettings>
</xml>
<![endif]-->
  <style type=""text/css"">
.rollover:hover .rollover-first {
  max-height:0px!important;
  display:none!important;
}
.rollover:hover .rollover-second {
  max-height:none!important;
  display:block!important;
}
.rollover span {
  font-size:0px;
}
u + .body img ~ div div {
  display:none;
}
#outlook a {
  padding:0;
}
span.MsoHyperlink,
span.MsoHyperlinkFollowed {
  color:inherit;
  mso-style-priority:99;
}
a.es-button {
  mso-style-priority:100!important;
  text-decoration:none!important;
}
a[x-apple-data-detectors],
#MessageViewBody a {
  color:inherit!important;
  text-decoration:none!important;
  font-size:inherit!important;
  font-family:inherit!important;
  font-weight:inherit!important;
  line-height:inherit!important;
}
.es-desk-hidden {
  display:none;
  float:left;
  overflow:hidden;
  width:0;
  max-height:0;
  line-height:0;
  mso-hide:all;
}
@media only screen and (max-width:600px) {.es-m-p20b { padding-bottom:20px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-p-default { } *[class=""gmail-fix""] { display:none!important } p, a { line-height:150%!important } h1, h1 a { line-height:120%!important } h2, h2 a { line-height:120%!important } h3, h3 a { line-height:120%!important } h4, h4 a { line-height:120%!important } h5, h5 a { line-height:120%!important } h6, h6 a { line-height:120%!important } .es-header-body p { } .es-content-body p { } .es-footer-body p { } .es-infoblock p { } h1 { font-size:36px!important; text-align:left } h2 { font-size:26px!important; text-align:left } h3 { font-size:20px!important; text-align:left } h4 { font-size:24px!important; text-align:left } h5 { font-size:20px!important; text-align:left } h6 { font-size:16px!important; text-align:left } .es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a { font-size:36px!important } .es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a { font-size:26px!important } .es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a { font-size:20px!important } .es-header-body h4 a, .es-content-body h4 a, .es-footer-body h4 a { font-size:24px!important } .es-header-body h5 a, .es-content-body h5 a, .es-footer-body h5 a { font-size:20px!important } .es-header-body h6 a, .es-content-body h6 a, .es-footer-body h6 a { font-size:16px!important } .es-menu td a { font-size:12px!important } .es-header-body p, .es-header-body a { font-size:14px!important } .es-content-body p, .es-content-body a { font-size:16px!important } .es-footer-body p, .es-footer-body a { font-size:14px!important } .es-infoblock p, .es-infoblock a { font-size:12px!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3, .es-m-txt-c h4, .es-m-txt-c h5, .es-m-txt-c h6 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3, .es-m-txt-r h4, .es-m-txt-r h5, .es-m-txt-r h6 { text-align:right!important } .es-m-txt-j, .es-m-txt-j h1, .es-m-txt-j h2, .es-m-txt-j h3, .es-m-txt-j h4, .es-m-txt-j h5, .es-m-txt-j h6 { text-align:justify!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3, .es-m-txt-l h4, .es-m-txt-l h5, .es-m-txt-l h6 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-m-txt-r .rollover:hover .rollover-second, .es-m-txt-c .rollover:hover .rollover-second, .es-m-txt-l .rollover:hover .rollover-second { display:inline!important } .es-m-txt-r .rollover span, .es-m-txt-c .rollover span, .es-m-txt-l .rollover span { line-height:0!important; font-size:0!important; display:block } .es-spacer { display:inline-table } a.es-button, button.es-button { font-size:20px!important; padding:10px 20px 10px 20px!important; line-height:120%!important } a.es-button, button.es-button, .es-button-border { display:inline-block!important } .es-m-fw, .es-m-fw.es-fw, .es-m-fw .es-button { display:block!important } .es-m-il, .es-m-il .es-button, .es-social, .es-social td, .es-menu { display:inline-block!important } .es-adaptive table, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .adapt-img { width:100%!important; height:auto!important } .es-mobile-hidden, .es-hidden { display:none!important } .es-desk-hidden {  overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } tr.es-desk-hidden { display:table-row!important } table.es-desk-hidden { display:table!important } td.es-desk-menu-hidden { display:table-cell!important } .es-menu td { width:1%!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } .h-auto { height:auto!important } .es-text-4746 .es-text-mobile-size-14, .es-text-4746 .es-text-mobile-size-14 * { font-size:14px!important; line-height:150%!important } .es-text-9171 .es-text-mobile-size-26, .es-text-9171 .es-text-mobile-size-26 * { font-size:26px!important; line-height:150%!important } .es-text-5335 .es-text-mobile-size-10.es-override-size, .es-text-5335 .es-text-mobile-size-10.es-override-size * { font-size:10px!important; line-height:150%!important } .es-text-5335 .es-text-mobile-size-12.es-override-size, .es-text-5335 .es-text-mobile-size-12.es-override-size * { font-size:12px!important; line-height:150%!important } .es-text-5335 .es-text-mobile-size-14.es-override-size, .es-text-5335 .es-text-mobile-size-14.es-override-size * { font-size:14px!important; line-height:150%!important } .es-text-9623 .es-text-mobile-size-14.es-override-size, .es-text-9623 .es-text-mobile-size-14.es-override-size * { font-size:14px!important; line-height:150%!important } }
@media screen and (max-width:384px) {.mail-message-content { width:414px!important } .hiddenHeader { display:none!important } }
</style>
 </head>" +
 $@"

 <body class=""body"" style=""width:100%;height:100%;padding:0;Margin:0"">
  <div dir=""ltr"" class=""es-wrapper-color"" lang=""en"" style=""background-color:#FAFAFA""><!--[if gte mso 9]>
			<v:background xmlns:v=""urn:schemas-microsoft-com:vml"" fill=""t"">
				<v:fill type=""tile"" color=""#fafafa""></v:fill>
			</v:background>
		<![endif]-->
   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" class=""es-wrapper"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#FAFAFA;"">
     <tr>
      <td valign=""top"" style=""padding:0;Margin:0"">
       <table cellpadding=""0"" cellspacing=""0"" align=""center"" class=""es-header"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0"">
           <table bgcolor=""#ffffff"" align=""center"" cellpadding=""0"" cellspacing=""0"" class=""es-header-body"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"">
             <tr class=""es-mobile-hidden"">
             <td align=""center"" style=""padding:0;Margin:0;font-size:0""><img src=""https://devapi.fekrahub.app/api/SchoolInfo/SchoolLogo1"" alt="""" width=""80"" class=""adapt-img"" style=""display:block;font-size:14px;border:0;outline:none;text-decoration:none;padding-top:10px;""></td>

             </tr>
           </table></td>
         </tr>
       </table><!--[if !mso]><!-- -->
       <table  cellpadding=""0"" cellspacing=""0"" class=""es-header es-desk-hidden hiddenHeader"" role=""none"" style=""display:none;overflow:hidden;width:100%;max-height:0;line-height:0;mso-hide:all;mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr>
          
             
   <td align=""center"" valign=""middle"" style=""padding:0;Margin:0""><img src=""https://devapi.fekrahub.app/api/SchoolInfo/SchoolLogo1"" alt=""Logo"" width=""80px"" style=""display:block;font-size:14px;border:0;outline:none;text-decoration:none;padding-top:10px;""></td>

            
         </tr>
       </table><!--<![endif]-->







<table cellpadding=""0"" cellspacing=""0"" align=""center"" class=""es-content"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:100%;table-layout:fixed !important"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0"">
           <table bgcolor=""#ffffff"" align=""center"" cellpadding=""0"" cellspacing=""0"" class=""es-content-body"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px"">
             <tr>
              <td align=""left"" style=""Margin:0;padding-right:20px;padding-left:20px;padding-top:30px;padding-bottom:10px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""center"" style=""padding:0;Margin:0;padding-bottom:10px;padding-top:10px;font-size:0px""><img src=""https://enpbppa.stripocdn.email/content/guids/CABINET_a3448362093fd4087f87ff42df4565c1/images/78501618239341906.png"" alt="""" width=""100"" style=""display:block;font-size:14px;border:0;outline:none;text-decoration:none""></td>
                     </tr>



                     <tr>
                      <td align=""center"" style=""padding:20px;Margin:30px 0;;font-size:14px"">
                      
                          {contentHtml}
                        
                        </td>
                     </tr>



                   </table>
                    </td>
                 </tr>
               </table>
                </td>
             </tr>
            
           </table>
            </td>
         </tr>
       </table>




 <table cellpadding=""0"" cellspacing=""0"" align=""center"" class=""es-footer"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0"">
           <table align=""center"" cellpadding=""0"" cellspacing=""0"" class=""es-footer-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:640px"" role=""none"">
             <tr>
              <td align=""center"" style=""Margin:0;padding-top:20px;padding-right:20px;padding-left:20px;padding-bottom:20px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" style=""padding:0;Margin:0;width:600px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""left"" class=""es-text-4746"" style=""padding:0;Margin:0;padding-bottom:10px;"">
<table bgcolor=""#ffffff"" align=""center"" cellpadding=""0"" cellspacing=""0"" class="""" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:100%"">
             <tr>
<td align=""right"" style=""padding:0;Margin:0;padding-right:10px;padding-bottom:10px;width:150px;"">
powered by
</td>

<td align=""left"" style=""padding:0;Margin:0;width:150px;"" >
<img src=""https://devapi.fekrahub.app/api/SchoolInfo/SchoolLogo2"" alt="""" width=""40"" style=""border:0;outline:none;text-decoration:none"">
</td>

             </tr>
           </table>
 </td>
                     </tr>
<tr>
                      <td align=""center"" class=""es-text-4746"" style=""padding:0;Margin:0;padding-bottom:10px;"">
<p class=""es-text-mobile-size-14"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">© 2024 <a target=""_blank"" href=""https://netwitcher.com"" style=""mso-line-height-rule:exactly;text-decoration:none;color:#333333;font-size:14px;line-height:21px"">NetWitcher</a>. Alle Rechte vorbehalten.</p>
                      </td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table></td>
     </tr>
   </table>
  </div>
 </body>
</html>
";
            return ConstantsMessage;
        }


    }
}
