using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using MailKit.Net.Smtp;
using MimeKit;
using FekraHubAPI.Controllers.AuthorizationController;
using FekraHubAPI.Constract;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Macs;

namespace FekraHubAPI.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<StudentContract> _studentContract;
        private readonly IRepository<SchoolInfo> _schoolInfo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IConfiguration _configuration;
        public EmailSender(IRepository<SchoolInfo> schoolInfo, UserManager<ApplicationUser> userManager,
            IRepository<Student> studentRepo, IRepository<StudentContract> studentContract, ApplicationDbContext context,
            IConfiguration configuration, IRepository<Course> courseRepo)
        {
            _schoolInfo = schoolInfo;
            _userManager = userManager;
            _studentRepo = studentRepo;
            _studentContract = studentContract;
            _context = context;
            _configuration = configuration;
            _courseRepo = courseRepo;
        }

        private async Task SendEmail(string toEmail, string subject, string body, bool isBodyHTML, string? submessage = "", byte[]? pdf = null, string? pdfName = null)
        {
            var schoolInfo = await _context.SchoolInfos
                .Select(x => new { x.EmailServer, x.EmailPortNumber, x.FromEmail, x.Password, x.SchoolName })
                .FirstAsync();
            string MailServer = schoolInfo.EmailServer;
            int Port = schoolInfo.EmailPortNumber;
            string FromEmail = schoolInfo.FromEmail;
            string Password = schoolInfo.Password;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(schoolInfo.SchoolName, FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = isBodyHTML ? body : null,
                TextBody = !isBodyHTML ? body : null
            };

            if (pdf != null)
            {
                bodyBuilder.Attachments.Add(pdfName ?? "pdf.pdf", pdf, new ContentType("application", "pdf"));
            }

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(MailServer, Port, MailKit.Security.SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(FromEmail, Password);
                    await client.SendAsync(message);
                }
                catch (SmtpCommandException ex) when (ex.ErrorCode == SmtpErrorCode.SenderNotAccepted)
                {
                    try
                    {
                        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.Auto);
                        await client.AuthenticateAsync("abog9022@gmail.com", "xhuzbgwifkyajsms");
                        await client.SendAsync(message);

                        var AdminIds = await _context.UserRoles.Where(x => x.RoleId == "1").Select(x => x.UserId).ToListAsync();
                        var admins = await _userManager.Users.Where(x => AdminIds.Contains(x.Id)).ToListAsync();
                        if (admins.Any())
                        {
                            foreach (var admin in admins)
                            {
                                var notificationMessage = new MimeMessage();
                                notificationMessage.From.Add(new MailboxAddress(schoolInfo.SchoolName, "abog9022@gmail.com"));
                                notificationMessage.To.Add(new MailboxAddress("", admin.Email));
                                notificationMessage.Subject = "Failed to use school email";
                                notificationMessage.Body = new TextPart("plain")
                                {
                                    Text = $"Failed to send email by {FromEmail}.\nNow we use alternative email."
                                };
                                await client.SendAsync(notificationMessage);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                catch (SmtpCommandException ex) when (ex.ErrorCode == SmtpErrorCode.RecipientNotAccepted)
                {
                    try
                    {
                        if (submessage != "")
                        {
                            var AdminIds = await _context.UserRoles.Where(x => x.RoleId == "1").Select(x => x.UserId).ToListAsync();
                            var admins = await _userManager.Users.Where(x => AdminIds.Contains(x.Id)).ToListAsync();
                            if (admins.Any())
                            {
                                foreach (var admin in admins)
                                {
                                    var notificationMessage = new MimeMessage();
                                    notificationMessage.From.Add(new MailboxAddress(schoolInfo.SchoolName, FromEmail));
                                    notificationMessage.To.Add(new MailboxAddress("", admin.Email));
                                    notificationMessage.Subject = "Failed to deliver email";
                                    notificationMessage.Body = new TextPart("plain")
                                    {
                                        Text = $"Failed to send email to {toEmail}. {submessage}."
                                    };
                                    await client.SendAsync(notificationMessage);
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                catch (Exception)
                {

                }
                finally
                {

                    try
                    {
                        await client.DisconnectAsync(true);
                    }
                    catch
                    {

                    }
                }
            }

        }


        private string Message(string contentHtml)
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
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
   <table width=""100%"" cellspacing=""0"" cellpadding=""0"" class=""es-wrapper"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#FAFAFA"">
     <tr>
      <td valign=""top"" style=""padding:0;Margin:0"">
       <table cellpadding=""0"" cellspacing=""0"" align=""center"" class=""es-header"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0"">
           <table bgcolor=""#ffffff"" align=""center"" cellpadding=""0"" cellspacing=""0"" class=""es-header-body"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px"">
             <tr class=""es-mobile-hidden"">
              <td align=""left"" style=""padding:0;Margin:0;padding-top:20px;padding-right:20px;padding-left:20px""><!--[if mso]><table style=""width:560px"" cellpadding=""0"" cellspacing=""0""><tr><td style=""width:270px"" valign=""top""><![endif]-->
               <table cellpadding=""0"" cellspacing=""0"" align=""left"" class=""es-left"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:left"">
                 <tr>
                  <td align=""left"" class=""es-m-p20b"" style=""padding:0;Margin:0;width:270px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""left"" style=""padding:0;Margin:0""><h2 style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:78px !important;color:#333333"">{schoolName}</h2></td>
                     </tr>
                   </table></td>
                 </tr>
               </table><!--[if mso]></td><td style=""width:20px""></td><td style=""width:270px"" valign=""top""><![endif]-->
               <table cellpadding=""0"" cellspacing=""0"" align=""right"" class=""es-right"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:right"">
                 <tr>
                  <td align=""left"" style=""padding:0;Margin:0;width:270px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""right"" style=""padding:0;Margin:0;font-size:0""><img src=""https://api.fekrahub.com/api/SchoolInfo/SchoolLogo"" alt="""" width=""80"" class=""adapt-img"" style=""display:block;font-size:14px;border:0;outline:none;text-decoration:none""></td>
                     </tr>
                   </table></td>
                 </tr>
               </table><!--[if mso]></td></tr></table><![endif]--></td>
             </tr>
           </table></td>
         </tr>
       </table><!--[if !mso]><!-- -->
       <table  cellpadding=""0"" cellspacing=""0"" class=""es-header es-desk-hidden hiddenHeader"" role=""none"" style=""display:none;overflow:hidden;width:100%;max-height:0;line-height:0;mso-hide:all;mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr>
          
              <td align=""left"" valign=""middle"" style=""padding:0;Margin:0;"">
               <table width=""45%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td valign=""middle"" align=""left"" style=""padding:0;Margin:0;padding-left:10px;"">
<h3 style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:31px;color:#333333"">{schoolName}</h3></td>
                 </tr>
               </table>
            </td>
            <td align=""right"" valign=""middle"" style=""padding:0;Margin:0;"">
                <table width=""45%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""right"" valign=""middle"" style=""padding:0;Margin:0""><img src=""https://api.fekrahub.com/api/SchoolInfo/SchoolLogo"" alt=""Logo"" width=""80px"" style=""display:block;font-size:14px;border:0;outline:none;text-decoration:none""></td>
                 </tr>
               </table>
            </td>
            
         </tr>
       </table><!--<![endif]-->

{contentHtml}

 <table cellpadding=""0"" cellspacing=""0"" align=""center"" class=""es-footer"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:100%;table-layout:fixed !important;background-color:transparent;background-repeat:repeat;background-position:center top"">
         <tr>
          <td align=""center"" style=""padding:0;Margin:0"">
           <table align=""center"" cellpadding=""0"" cellspacing=""0"" class=""es-footer-body"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:640px"" role=""none"">
             <tr>
              <td align=""left"" style=""Margin:0;padding-top:20px;padding-right:20px;padding-left:20px;padding-bottom:20px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""left"" style=""padding:0;Margin:0;width:600px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""center"" class=""es-text-4746"" style=""padding:0;Margin:0;padding-bottom:35px""><p class=""es-text-mobile-size-14"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">© 2024 <a target=""_blank"" href=""https://netwitcher.com"" style=""mso-line-height-rule:exactly;text-decoration:none;color:#333333;font-size:14px;line-height:21px"">NetWitcher</a>. All rights reserved.</p></td>
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
        public async Task<IActionResult> SendConfirmationEmail(ApplicationUser user)
        {
            var school = _context.SchoolInfos.Select(x => new { x.SchoolName, x.UrlDomain }).Single();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{school.UrlDomain}/confirm-user?ID={user.Id}&Token={token}";
            var content = $@"

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
                      <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {user.FirstName} {user.LastName}</h2></td>
                     </tr>
                     <tr>
                      <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Welcome to {school.SchoolName} ! , Thank you For Confirming your Account, &nbsp;</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">The activation button is valid for <strong>&nbsp;7 days</strong> .</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Please activate the email before this period expires​. &nbsp;</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">To complete the confirmation, please click the confirm button. &nbsp;</p></td>
                     </tr>
                     <tr>
                      <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                       <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;width:100%;margin:0px;background:none;height:1px""></td>
                         </tr>
                       </table></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
             <tr>
              <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                     <tr>
                      <td align=""center"" style=""padding:0;Margin:0;padding-bottom:10px;padding-top:10px""><span class=""es-button-border"" style=""border-style:solid;border-color:#2CB543;background:#5C68E2;border-width:0px;display:inline-block;border-radius:6px;width:auto""><a href=""{confirmationLink}"" target=""_blank"" class=""es-button"" style=""mso-style-priority:100 !important;text-decoration:none !important;mso-line-height-rule:exactly;color:#FFFFFF;font-size:20px;padding:10px 30px 10px 30px;display:inline-block;background:#5C68E2;border-radius:6px;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-weight:normal;font-style:normal;line-height:24px;width:auto;text-align:center;letter-spacing:0;mso-padding-alt:0;mso-border-alt:10px solid #5C68E2;padding-left:30px;padding-right:30px""> Confirm</a></span></td>
                     </tr>
                     <tr>
                      <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                       <table height=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;height:1px;width:100%;margin:0px;background:none""></td>
                         </tr>
                       </table></td>
                     </tr>
                     <tr>
                      <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-14 es-override-size"">Got a question?</span></p><p class=""es-text-mobile-size-10 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Email us at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px""> Admin@fekraschule.de </a> or give us a call at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px;line-height:21px""> 01794169927 </a> .</p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time.</span></p></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>

 ";
            try
            {
                await SendEmail(user.Email ?? "", "Please Confirm Your Email", Message(content), true);
                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return new BadRequestObjectResult($"Error sending email: {ex.Message}");
            }
        }
        public async Task<IActionResult> SendConfirmationEmailWithPassword(ApplicationUser user, string password)
        {
            var school = _context.SchoolInfos.Select(x => new { x.SchoolName, x.UrlDomain }).Single();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{school.UrlDomain}/confirm-user?ID={user.Id}&Token={token}";
            var content = $@"

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
                      <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {user.FirstName} {user.LastName}</h2></td>
                     </tr>
                     <tr>
                      <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">Welcome to {school.SchoolName} ! , Thank you For Confirming your Account, &nbsp;</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">The activation button is valid for <strong>&nbsp;7 days</strong> .</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Please activate the email before this period expires​. &nbsp;</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">To complete the confirmation, please click the confirm button. &nbsp;</p></td>
                     </tr>
                     <tr>
                      <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                       <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;width:100%;margin:0px;background:none;height:1px""></td>
                         </tr>
                       </table></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
            
            <tr>
              <td align=""left"" style=""padding:0;Margin:0;padding-top:20px;padding-right:20px;padding-left:60px"">
               <table cellspacing=""0"" width=""100%"" cellpadding=""0"" align=""right"" class=""es-right"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:right"">
                 <tr>
                  <td align=""left"" style=""padding:0;Margin:0;width:520px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                     <tr>
                      <td align=""left"" style=""padding:0;Margin:0;padding-bottom:15px""><h3 style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:20px;font-style:normal;font-weight:bold;line-height:24px;color:#333333"">Login Details</h3></td>
                     </tr>
                     <tr>
                      <td align=""left"" style=""padding:0;Margin:0""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Email : <strong>{user.Email}</strong></p></td>
                     </tr>
                     <tr>
                      <td align=""left"" style=""padding:0;Margin:0""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Password : <strong>{password}</strong></p></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>


             <tr>
              <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                     <tr>
                      <td align=""center"" style=""padding:0;Margin:0;padding-bottom:10px;padding-top:10px""><span class=""es-button-border"" style=""border-style:solid;border-color:#2CB543;background:#5C68E2;border-width:0px;display:inline-block;border-radius:6px;width:auto""><a href=""{confirmationLink}"" target=""_blank"" class=""es-button"" style=""mso-style-priority:100 !important;text-decoration:none !important;mso-line-height-rule:exactly;color:#FFFFFF;font-size:20px;padding:10px 30px 10px 30px;display:inline-block;background:#5C68E2;border-radius:6px;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-weight:normal;font-style:normal;line-height:24px;width:auto;text-align:center;letter-spacing:0;mso-padding-alt:0;mso-border-alt:10px solid #5C68E2;padding-left:30px;padding-right:30px""> Confirm</a></span></td>
                     </tr>
                     <tr>
                      <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                       <table height=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;height:1px;width:100%;margin:0px;background:none""></td>
                         </tr>
                       </table></td>
                     </tr>
                     <tr>
                      <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-14 es-override-size"">Got a question?</span></p><p class=""es-text-mobile-size-10 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Email us at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px""> Admin@fekraschule.de </a> or give us a call at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px;line-height:21px""> 01794169927 </a> .</p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time.</span></p></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>
                            ";
            try
            {
                await SendEmail(user.Email ?? "", "Please Confirm Your Email", Message(content), true, "confirming an account link + login details");
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error sending email: {ex.Message}");
            }
        }

        public async Task<IActionResult> SendContractEmail(int studentId, string pdfName)//
        {
            var student = await _studentRepo.GetById(studentId);
            var parent = await _userManager.FindByIdAsync(student.ParentID ?? "");
            if (student == null || parent == null)
            {
                return new BadRequestObjectResult("Something seems wrong. Please re-register your child or contact us");
            }
            var contracts = await _studentContract.GetAll();
            byte[] contract = contracts.Where(x => x.StudentID == studentId).Select(x => x.File).First();
            var content = @$"
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
                     <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {parent.FirstName} {parent.LastName}</h2></td>
                    </tr>
                    <tr>
                        <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px"">
                          <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Your son <b>{student.FirstName} {student.LastName}</b> has been registered successfully. &nbsp;</p>
                          <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">A copy of the contract has been sent to you. &nbsp;</p>
                          <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">For more information contact us &nbsp;</p>
                        </td>
                    </tr>
                    <tr>
                     <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                      <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td style=""padding:0;Margin:0;width:100%;margin:0px;background:none;height:1px""></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table></td>
            </tr>
            <tr>
             <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
              <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                <tr>
                 <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                  <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                    
                    <tr>
                     <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                      <table height=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td style=""padding:0;Margin:0;height:1px;width:100%;margin:0px;background:none""></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-14 es-override-size"">Got a question?</span></p><p class=""es-text-mobile-size-10 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Email us at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px""> Admin@fekraschule.de </a> or give us a call at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px;line-height:21px""> 01794169927 </a> .</p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time.</span></p></td>
                    </tr>
                  </table></td>
                </tr>
              </table></td>
            </tr>
          </table></td>
        </tr>
      </table>
";
            try
            {
                await SendEmail(parent.Email ?? "", "Registration Confirmation", Message(content), true, $"a new student named <b>{student.FirstName} {student.LastName}</b> has been registered successfully + copy of the contract", contract, pdfName + ".pdf");
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error sending email: {ex.Message}");
            }
        }

        public async Task SendToAdminNewParent(ApplicationUser user)//////////////
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var AdminIds = _context.UserRoles.Where(x => x.RoleId == "1").Select(x => x.UserId).ToList();
            var admins = _userManager.Users.Where(x => AdminIds.Contains(x.Id)).ToList();
            foreach (var admin in admins)
            {


                var content = @$"

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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {admin.FirstName} {admin.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">{schoolName} would like to tell you some new information about your school ,</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">A new family has been added .</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">​</p><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap""><strong>family information :</strong></p>
                              <ul style=""font-family:arial, 'helvetica neue', helvetica, sans-serif;padding:0px 0px 0px 40px;margin:15px 0px;white-space:nowrap"">
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;mso-margin-top-alt:15px"">Name : {user.FirstName} {user.LastName}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Email : {user.Email}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">PhoneNumber : {user.PhoneNumber} / {user.EmergencyPhoneNumber}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Gender : {user.Gender}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Birthday : {user.Birthday}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Birthplace : {user.Birthplace}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Nationality : {user.Nationality}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Job : {user.Job}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">City : {user.City}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Street :{user.Street} {user.StreetNr}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;mso-margin-bottom-alt:15px"">ZipCode : {user.ZipCode}</p></li>
                              </ul></td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>
";
                await SendEmail(admin?.Email ?? "", "New User Registered", Message(content), true);
            }
        }
        public async Task SendToAdminNewStudent(Student student)//////////////
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var user = _userManager.Users.Where(x => x.Id == student.ParentID).Single();
            var courseSelectedName = _context.Courses.Where(x => x.Id == student.CourseID).Single();
            var AdminIds = _context.UserRoles.Where(x => x.RoleId == "1").Select(x => x.UserId).ToList();
            var admins = _userManager.Users.Where(x => AdminIds.Contains(x.Id)).ToList();
            foreach (var admin in admins)
            {


                var content = @$"

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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {admin.FirstName} {admin.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">{schoolName} would like to tell you some new information about your school ,</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">A new student has been added .</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">​</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap""><strong>Parent information :</strong></p>
                              <ul style=""font-family:arial, 'helvetica neue', helvetica, sans-serif;padding:0px 0px 0px 40px;margin:15px 0px;white-space:nowrap"">
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;mso-margin-top-alt:15px"">Name : {user.FirstName} {user.LastName}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Email : {user.Email}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Number : {user.PhoneNumber} / {user.EmergencyPhoneNumber}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Gender : {user.Gender}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Birthday : {user.Birthday}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Birthplace : {user.Birthplace}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Nationality : {user.Nationality}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Job : {user.Job}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">City : {user.City}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Street :{user.Street} {user.StreetNr}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;mso-margin-bottom-alt:15px"">ZipCode : {user.ZipCode}</p></li>
                              </ul>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap""><strong>student information :</strong></p>
                              <ul style=""font-family:arial, 'helvetica neue', helvetica, sans-serif;padding:0px 0px 0px 40px;margin:15px 0px;white-space:nowrap"">
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;mso-margin-top-alt:15px"">Name : {student.FirstName} {student.LastName}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Gender : {student.Gender}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Birthday : {student.Birthday}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Nationality : {student.Nationality}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">City : {student.City}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Street :{student.Street} {student.StreetNr}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">ZipCode : {student.ZipCode}</p></li>
                               <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;mso-margin-bottom-alt:15px"">Course Selected : {courseSelectedName}</p></li>
                              </ul>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>
";
                await SendEmail(admin?.Email ?? "", "New student Registered", Message(content), true);
            }
        }

        public async Task SendToAllNewEvent(List<int?> corsesId)//////////////
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var ParentIds = await _context.Students.Where(x => corsesId.Contains(x.CourseID))
                .Select(x => x.ParentID).ToListAsync();
            var TeacherIds = await _context.Courses.Where(x => corsesId.Contains(x.Id))
                .SelectMany(x => x.Teacher.Select(z => z.Id)).ToListAsync();
            var NotTeacherId = await _context.UserRoles
                .Where(x => x.RoleId == "4" && !TeacherIds.Contains(x.UserId)).Select(x => x.UserId).ToListAsync();
            var AdminSecrTeacher = await _userManager.Users
                .Where(z => !NotTeacherId.Contains(z.Id) && !ParentIds.Contains(z.Id))
                .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email }).ToListAsync();



            var students = (await _studentRepo.GetRelation<Student>(
                x => corsesId.Contains(x.CourseID))).Select(z => new {
                    z.FirstName,
                    z.LastName,
                    User = new
                    {
                        z.User.FirstName,
                        z.User.LastName,
                        z.User.Email
                    }

                }).ToList();
            foreach (var student in students)
            {



                var content = @$"


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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {student.User.FirstName} {student.User.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">{schoolName} would like to tell you some new information about your children</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">​</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap""><strong>Children Name :</strong></p>
                              <ul style=""font-family:arial, 'helvetica neue', helvetica, sans-serif;padding:0px 0px 0px 40px;margin:15px 0px;white-space:nowrap"">
                              <li style=""color:#333333;margin:0px 0px 15px;font-size:14px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;mso-margin-top-alt:15px"">{student.FirstName} {student.LastName}</p></li>
                              </ul>
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">A new event has been added .</p>
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">For more information, please go to the events page on our official website or click the button to be directed to the event's page<a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px;line-height:21px""> event's page </a></p>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>
            
";
                await SendEmail(student.User.Email ?? "", "New Event", Message(content), true, "A new event has been added");

            }
            foreach (var user in AdminSecrTeacher)
            {
                var content = @$" <table cellpadding=""0"" cellspacing=""0"" align=""center"" class=""es-content"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:100%;table-layout:fixed !important"">
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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {user.FirstName} {user.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px"">
                            <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">{schoolName} would like to tell you some new information about your students</p>
                             
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">A new event has been added .</p>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>

";
                await SendEmail(user.Email ?? "", "", Message(content), true, "A new event has been added");
            }

        }

        public async Task SendToParentsNewFiles(int coursId)/////////////////////
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var course = await _context.Courses.FindAsync(coursId);
            if (course != null)
            {
                var students = await _context.Students.Where(x => x.CourseID == coursId).ToListAsync();
                var parents = await _userManager.Users
               .Where(x => students.Select(z => z.ParentID).Contains(x.Id))
               .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email })
               .ToListAsync();

                foreach (var student in students)
                {
                    var parent = parents.Where(x => x.Id == student.ParentID).FirstOrDefault();
                    if (parent == null)
                    {
                        continue;
                    }

                    var content = @$"

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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {parent.FirstName} {parent.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px"">
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">{schoolName} would like to tell you some new information about your children</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">​</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap""><strong>Children Name :</strong></p>
                              <ul style=""font-family:arial, 'helvetica neue', helvetica, sans-serif;padding:0px 0px 0px 40px;margin:15px 0px;white-space:nowrap"">
                              <li>{student.FirstName} {student.LastName}</li>
                              </ul>
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">A new file has been added .</p>
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">For more information, please go to the events page on our official <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px;line-height:21px""> website</a></p>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>

";
                    await SendEmail(parent.Email ?? "", "New Files", Message(content), true, "A new file has been added ");
                }
            }


        }

        public async Task SendToSecretaryNewReportsForStudents()
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var SecretariesId = await _context.UserRoles
                            .Where(x => x.RoleId == "2")
                            .Select(x => x.UserId)
                            .ToListAsync();
            var Secretaries = await _userManager.Users
                         .Where(user => SecretariesId.Contains(user.Id))
                         .ToListAsync();
            foreach (var Secretary in Secretaries)
            {
                var content = @$"
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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {Secretary.FirstName} {Secretary.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px"">
                            <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">{schoolName} would like to tell you some new information about your students</p>
                             
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">New reports has been added .</p>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>

";
                await SendEmail(Secretary.Email, "", Message(content), true, "New reports has been added .", null, null);
            }

        }
        public async Task SendToSecretaryUpdateReportsForStudents()
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var SecretariesId = await _context.UserRoles
                            .Where(x => x.RoleId == "2")
                            .Select(x => x.UserId)
                            .ToListAsync();
            var Secretaries = await _userManager.Users
                         .Where(user => SecretariesId.Contains(user.Id))
                         .ToListAsync();
            foreach (var Secretary in Secretaries)
            {
                var content = @$"
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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {Secretary.FirstName} {Secretary.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px"">
                            <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">{schoolName} would like to tell you some new information about your students</p>
                             
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">Some reports has been updated .</p>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>

";
                await SendEmail(Secretary.Email, "", Message(content), true, "Some reports has been updated .", null, null);
            }

        }
        public async Task SendToParentsNewReportsForStudents(List<Student> students)
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var parents = await _userManager.Users
                .Where(x => students.Select(z => z.ParentID).Contains(x.Id))
                .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email })
                .ToListAsync();

            foreach (var student in students)
            {
                var parent = parents.Where(x => x.Id == student.ParentID).FirstOrDefault();
                if (parent == null)
                {
                    continue;
                }
                var content = @$"

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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {parent.FirstName} {parent.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px"">
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">{schoolName} would like to tell you some new information about your children</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">​</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;""><strong>Children Name :</strong></p>
                              <ul style=""font-family:arial, 'helvetica neue', helvetica, sans-serif;padding:0px 0px 0px 40px;margin:15px 0px;white-space:nowrap"">
                              <li>{student.FirstName} {student.LastName}</li>
                              </ul>
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">A new report has been added .</p>
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">For more information, please go to the reports page on our official <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px;line-height:21px""> website</a></p>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>


";
                await SendEmail(parent.Email ?? "", "New Reports", Message(content), true, "A new report has been added .");



            }
        }

        public async Task SendToTeacherReportsForStudentsNotAccepted(int studentId, string teacherId)
        {
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single();
            var student = await _studentRepo.GetById(studentId);
            if (await _studentRepo.IsTeacherIDExists(teacherId))
            {
                var teacher = await _userManager.FindByIdAsync(teacherId);
                var content = @$"
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
                             <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {teacher.FirstName} {teacher.LastName}</h2></td>
                            </tr>
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px"">
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">{schoolName} would like to tell you some new information about your students</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;"">​</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;""><strong>Student Name :</strong></p>
                              <ul style=""font-family:arial, 'helvetica neue', helvetica, sans-serif;padding:0px 0px 0px 40px;margin:15px 0px;white-space:nowrap"">
                              <li>{student.FirstName} {student.LastName}</li>
                              </ul>
                              <p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px;white-space:nowrap"">a report has been not accepteds .</p>
                            </td>
                            </tr>
                            <tr>
                             <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                                <tr>
                                 <td style=""padding:0;Margin:0;width:100%;margin:0px;border-bottom:1px solid #cccccc;background:none;height:1px""></td>
                                </tr>
                              </table></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                    <tr>
                     <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
                      <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                        <tr>
                         <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                          <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                            <tr>
                             <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time</span></p></td>
                            </tr>
                          </table></td>
                        </tr>
                      </table></td>
                    </tr>
                  </table></td>
                </tr>
              </table>

";
                await SendEmail(teacher.Email ?? "", "", Message(content), true, "a report has been not accepteds .");




            }
        }

        public async Task SendRestPassword(string email, string link)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var schoolName = _context.SchoolInfos.Select(x => x.SchoolName).Single(); ;
            var content = $@"
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
                      <td align=""center"" class=""es-m-txt-c es-text-9171"" style=""padding:0;Margin:0;padding-top:30px;padding-bottom:30px""><h2 class=""es-text-mobile-size-26"" style=""Margin:0;font-family:arial, 'helvetica neue', helvetica, sans-serif;mso-line-height-rule:exactly;letter-spacing:0;font-size:26px;font-style:normal;font-weight:bold;line-height:26px;color:#333333"">Hello {user.FirstName} {user.LastName}</h2></td>
                     </tr>
                     <tr>
                      <td align=""left"" class=""es-m-p0r es-m-p0l es-text-9623"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Welcome to {schoolName}! &nbsp;</p>
<p class=""es-text-mobile-size-14 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">To complete forget password, please click the <strong>&nbsp;button</strong> .</p>
                     </tr>
                     <tr>
                      <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                       <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" height=""100%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;width:100%;margin:0px;background:none;height:1px""></td>
                         </tr>
                       </table></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
             <tr>
              <td align=""left"" style=""padding:0;Margin:0;padding-right:20px;padding-left:20px;padding-bottom:30px"">
               <table cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""none"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                 <tr>
                  <td align=""center"" valign=""top"" style=""padding:0;Margin:0;width:560px"">
                   <table cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:5px"" role=""presentation"">
                     <tr>
                      <td align=""center"" style=""padding:0;Margin:0;padding-bottom:10px;padding-top:10px""><span class=""es-button-border"" style=""border-style:solid;border-color:#2CB543;background:#5C68E2;border-width:0px;display:inline-block;border-radius:6px;width:auto""><a href=""{link}"" target=""_blank"" class=""es-button"" style=""mso-style-priority:100 !important;text-decoration:none !important;mso-line-height-rule:exactly;color:#FFFFFF;font-size:20px;padding:10px 30px 10px 30px;display:inline-block;background:#5C68E2;border-radius:6px;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-weight:normal;font-style:normal;line-height:24px;width:auto;text-align:center;letter-spacing:0;mso-padding-alt:0;mso-border-alt:10px solid #5C68E2;padding-left:30px;padding-right:30px""> Click</a></span></td>
                     </tr>
                     <tr>
                      <td align=""center"" style=""padding:20px;Margin:0;font-size:0"">
                       <table height=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""5%"" class=""es-spacer"" role=""presentation"" style=""mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px"">
                         <tr>
                          <td style=""padding:0;Margin:0;height:1px;width:100%;margin:0px;background:none""></td>
                         </tr>
                       </table></td>
                     </tr>
                     <tr>
                      <td align=""left"" class=""es-m-p0r es-m-p0l es-text-5335"" style=""Margin:0;padding-top:5px;padding-right:40px;padding-bottom:5px;padding-left:40px""><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-14 es-override-size"">Got a question?</span></p><p class=""es-text-mobile-size-10 es-override-size"" style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px"">Email us at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px""> Admin@fekraschule.de </a> or give us a call at <a target=""_blank"" href="""" style=""mso-line-height-rule:exactly;text-decoration:underline;color:#5C68E2;font-size:14px;line-height:21px""> 01794169927 </a> .</p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><br></p><p style=""Margin:0;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;letter-spacing:0;color:#333333;font-size:14px""><span class=""es-text-mobile-size-12 es-override-size"">Thank you for your time.</span></p></td>
                     </tr>
                   </table></td>
                 </tr>
               </table></td>
             </tr>
           </table></td>
         </tr>
       </table>

                    ";
            await SendEmail(email ?? "", "Reset Password", Message(content), true, "forget password link");
        }


    }
}
