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

        private async Task SendEmail(string toEmail, string subject, string body, bool isBodyHTML, byte[]? pdf = null, string? pdfName = null)
        {
            var schoolInfo = await _context.SchoolInfos.FirstAsync();
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
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                    throw;
                }
            }
        }


        private string Message(string contentHtml)
        {
            var schoolName = _context.SchoolInfos.First().SchoolName;
            string ConstantsMessage = @$"
                   <!DOCTYPE html>
                   <html lang='en'>
                   <head>
                       <meta charset='UTF-8'>
                       <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                   </head>
                   <body>  
                    <table width='100%' style='background-color:rgb(242, 242, 242);'>
                       <tr>
                        <td style='width: 50%;'></td>
                        <td style='width: 300px; min-width: 300px;'>            
                            <table width='100%'>
                                <tr>
                                    <td align='left' valign='middle'>
                                        <h2>{schoolName}</h2>
                                    </td>
                                    <td align='right' valign='middle'>
                                        <img src='https://api.fekrahub.com/api/SchoolInfo/SchoolLogo' alt='Logo' width='80px'/>
                                    </td>
                                </tr>
                            </table>

                           <table>
                            <tr><td align='center' valign='middle' style='background-color:rgb(255, 255, 255);padding:10px;'>
                            {contentHtml}
                            <td><tr>
                            </table>

                           <table width='100%'>
                            <tr><td align='center' valign='middle' >
                                © 2024 NetWitcher. All rights reserved.
                            <td><tr>
                            </table>
                        </td>
                        <td style='width: 50%;'></td>
                    </tr>
                </table>
                </body>
                </html>            ";
            return ConstantsMessage;
        }
        public async Task<IActionResult> SendConfirmationEmail(ApplicationUser user)
        {

            var domain = (await _schoolInfo.GetRelation()).Select(x => x.UrlDomain).First();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{domain}/confirm-user?ID={user.Id}&Token={token}";
            var content = $@"
                            <table>
                            <tr><td align='center' valign='middle' >
                            <h1 style='width:100%;text-align:center;'>Hello {user.FirstName} {user.LastName}</h1>
                            <td><tr>
                            </table>

                            <table>
                            <tr><td align='left' valign='middle' style='padding:8px;'>
                             <p style='font-size:14px;'>Welcome to FekraHup ! , Thank you For Confirming your Account,</p>
                             <p style='font-size:14px;'>The activation button is valid for <b> 7 Days</b>. Please activate the email before this period expires</p>
                             <p style='font-size:14px;'>To complete the confirmation, please click the confirm button</p><br><br/>
                            <td><tr>
                            </table>

                            <table>
                            <tr><td align='center' valign='middle'>
                            <a href='{confirmationLink}' style='display: inline-block; text-decoration: none; color: white; padding: 10px; border: none; border-radius: 4px; background-color: rgb(83, 136, 247); text-align: center; cursor: pointer;'>
                                <h3 style='margin: 0;'>Confirm</h3>
                            </a>
                            <br></br>
                            <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p>
                            <td><tr>
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
            var domain = (await _schoolInfo.GetRelation()).Select(x => x.UrlDomain).First();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{domain}/confirm-user?ID={user.Id}&Token={token}";
            var content = $@"<div style='width:100%;text-align:center;'>
                            <h1 style='width:100%;text-align:center;'>Hello {user.FirstName} {user.LastName}</h1>
                             <p style='font-size:14px;'>Welcome to FekraHup!, Thank you For Confirming your Account,</p>
                             <p style='font-size:14px;'>The activation button is valid for <b> 7 Days</b>. Please activate the email before this period expires</p>
                            <p style='font-size:14px;'>To complete the confirmation, please click the confirm button</p><br><br/>
                            <div style='width:100%;text-align:center'> <a href='{confirmationLink}' style='text-decoration: none;color: white;padding: 2px 25px;border: none;border-radius: 4px;background-color: rgb(83, 136, 247);'><h3>confirm<h3></a></div>
                            <br><br/><br><br/><p style='font-size:14px;'><b>Login Details</b></p>
                            <p style='font-size:12px;'>E-mail : <b><span>{user.Email}</span></b></p>
                            <p style='font-size:12px;'>Password : <b>{password}</b></p>
                            <div style='width:100%;text-align:center'><p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div> </div>
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
            var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {parent.FirstName} {parent.LastName}</h1><hr></hr><br></br>
                         <p style='font-size:13px;'>Your son <b>{student.FirstName} {student.LastName}</b> has been registered successfully.</p>
                         <p style='font-size:13px;'>A copy of the contract has been sent to you. <br></br>
                        For more information contact us</p>
                        <div style='width:100%;text-align:center'><p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
            try
            {
                await SendEmail(parent.Email ?? "", "Registration Confirmation", Message(content), true, contract, pdfName + ".pdf");
                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return new BadRequestObjectResult($"Error sending email: {ex.Message}");
            }
        }

        public async Task SendToAdminNewParent(ApplicationUser user)
        {
            var AdminId = await _context.UserRoles.Where(x => x.RoleId == "1").Select(x => x.UserId).FirstOrDefaultAsync();
            var admin = await _userManager.Users.Where(x => x.Id == AdminId).FirstOrDefaultAsync();
            var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {admin.FirstName} {admin.LastName}</h1><hr></hr><br></br>
                        <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your school</p>
                         <p style='font-size:14px;'><b>A new family has been added .</b></p>
                         <p style='font-size:14px;'>family information :</p>
                           <ul>
                                <li>
                               Name : {user.FirstName} {user.LastName}
                                </li>
                                <li>
                                Email : {user.Email}
                                </li>
                                <li>
                                PhoneNumber : {user.PhoneNumber} / {user.EmergencyPhoneNumber}
                                </li>
                                <li>
                                Gender : {user.Gender}
                                </li>
                                <li>
                                Birthday : {user.Birthday}
                                </li>
                                <li>
                                Birthplace : {user.Birthplace}
                                </li>
                                <li>
                                Nationality : {user.Nationality}
                                </li>
                                <li>
                                Job : {user.Job}
                                </li>
                                <li>
                                City : {user.City}
                                </li>
                                <li>
                                StreetNr :{user.Street} {user.StreetNr} 
                                </li>
                                <li>
                                ZipCode : {user.ZipCode}
                                </li>
                                
                                
                            </ul>
                           <div style='width:100%;text-align:center'>
                        <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
            await SendEmail(admin?.Email ?? "", "New User Registration", Message(content), true);
        }

        public async Task SendToAllNewEvent()
        {
            var users = await _userManager.Users.ToListAsync();
            var NotParentsId = await _context.UserRoles.Where(x => x.RoleId != "3").Select(x => x.UserId).ToListAsync();
            List<ApplicationUser> parent = users
                .Where(user => !NotParentsId.Contains(user.Id))
                .ToList();
            List<ApplicationUser> notParent = users
                .Where(user => NotParentsId.Contains(user.Id))
                .ToList(); 

            var students = await _studentRepo.GetAll();
            foreach (var user in parent)
            {
                var student = students.Where(x => x.ParentID == user.Id).ToList();
                if (student.Any())
                {
                    var childrenNames = "";
                    foreach (var child in student)
                    {
                        childrenNames += "<p 'font-size:14px;'>" + child.FirstName + " " + child.LastName + "</p>";
                    }

                    var content = @$"<div style='width:100%;text-align:center;'>
                            <h1 style='width:100%;text-align:center;'>Hello {user.FirstName} {user.LastName}</h1><hr></hr><br></br>
                            <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your children</p>
                            <p><b>Children Name :</b></p>
                            {childrenNames}
                             <p style='font-size:14px;'><b>A new event has been added .</b></p>
                            <p style='font-size:14px;'>For more information, please go to the events page on our official website or click the button to be directed to the page directly</p>
                           <br></br><div style='width:100%;text-align:center'> <a href='www.google.com' style='text-decoration: none;color: white;padding: 10px 25px;border: none;border-radius: 4px;font-size: 20px;background-color: rgb(83, 136, 247);'>event page</a>
                            <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                         </div>";
                    await SendEmail(user.Email ?? "", "New Event", Message(content), true);
                }
            }
            foreach (var user in notParent)
            {
                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {user.FirstName} {user.LastName}</h1><hr></hr><br></br>
                        <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your students</p>
                         <p style='font-size:14px;'><b>A new event has been added .</b></p>
                           <div style='width:100%;text-align:center'>
                        <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
                await SendEmail(user.Email ?? "", "", Message(content), true);
            }

        }

        public async Task SendToParentsNewFiles(List<Student> students)
        {
            var parents = await _userManager.Users.ToListAsync();

            foreach (var student in students)
            {
                var parent = parents.Where(x => x.Id == student.ParentID).FirstOrDefault();
                if (parent == null)
                {
                    continue;
                }

                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {parent.FirstName} {parent.LastName}</h1><hr></hr><br></br>
                        <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your children</p>
                         <p><b>Name : {student.FirstName} {student.LastName}</b></p>
                         <p style='font-size:14px;'><b>A new file has been added .</b></p>
                        <p style='font-size:14px;'>For more information, please go to the events page on our official website or click the button to be directed to the page directly</p>
                       <br></br><div style='width:100%;text-align:center'> <a href='www.google.com' style='text-decoration: none;color: white;padding: 10px 25px;border: none;border-radius: 4px;font-size: 20px;background-color: rgb(83, 136, 247);'>files page</a>
                        <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
                    await SendEmail(parent.Email ?? "", "New Files", Message(content), true);
                }

        }

        public async Task SendToSecretaryNewReportsForStudents()
        {
            var SecretariesId = await _context.UserRoles
                            .Where(x => x.RoleId == "2")
                            .Select(x => x.UserId)
                            .ToListAsync();
            var Secretaries = await _userManager.Users
                         .Where(user => SecretariesId.Contains(user.Id))
                         .ToListAsync();
            foreach (var Secretary in Secretaries)
            {
                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {Secretary.FirstName} {Secretary.LastName}</h1><hr></hr><br></br>
                        <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your students</p>
                         <p style='font-size:14px;'><b>New reports has been added .</b></p>
                           <div style='width:100%;text-align:center'>
                        <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
                await SendEmail(Secretary.Email, "", Message(content), true, null, null);
            }

        }
        public async Task SendToSecretaryUpdateReportsForStudents()
        {
            var SecretariesId = await _context.UserRoles
                            .Where(x => x.RoleId == "2")
                            .Select(x => x.UserId)
                            .ToListAsync();
            var Secretaries = await _userManager.Users
                         .Where(user => SecretariesId.Contains(user.Id))
                         .ToListAsync();
            foreach (var Secretary in Secretaries)
            {
                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {Secretary.FirstName} {Secretary.LastName}</h1><hr></hr><br></br>
                        <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your students</p>
                         <p style='font-size:14px;'><b>old reports has been updated .</b></p>
                           <div style='width:100%;text-align:center'>
                        <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
                await SendEmail(Secretary.Email, "", Message(content), true, null, null);
            }

        }
        public async Task SendToParentsNewReportsForStudents(List<Student> students)
        {
            var parents = await _userManager.Users.ToListAsync();
             
            foreach (var student in students)
            {
                var parent = parents.Where(x => x.Id == student.ParentID).FirstOrDefault();
                if (parent == null)
                {
                    continue;
                }
                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {parent.FirstName} {parent.LastName}</h1><hr></hr><br></br>
                        <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your children</p>
                        <p><b>Name : {student.FirstName} {student.LastName}</b></p>
                         <p style='font-size:14px;'><b>A new report has been added .</b></p>
                        <p style='font-size:14px;'>For more information, please go to the events page on our official website or click the button to be directed to the page directly</p>
                       <br></br><div style='width:100%;text-align:center'> <a href='www.google.com' style='text-decoration: none;color: white;padding: 10px 25px;border: none;border-radius: 4px;font-size: 20px;background-color: rgb(83, 136, 247);'>reports page</a>
                        <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
                await SendEmail(parent.Email ?? "", "New Reports", Message(content), true);



            }
        }

        public async Task SendToTeacherReportsForStudentsNotAccepted(int studentId,string teacherId)
        {
            var student = await _studentRepo.GetById(studentId);
            if (await _studentRepo.IsTeacherIDExists(teacherId))
            {
                var teacher = await _userManager.FindByIdAsync(teacherId);
                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1 style='width:100%;text-align:center;'>Hello {teacher.FirstName} {teacher.LastName}</h1><hr></hr><br></br>
                        <p style='font-size:14px;'>Fekra Hub would like to tell you some new information about your students</p>
                        <p><b>Student Name : {student.FirstName} {student.LastName}</b></p>
                         <p style='font-size:14px;'><b> report has been not accepteds .</b></p>
                        <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div>
                     </div>";
                await SendEmail(teacher.Email ?? "", "", Message(content), true);




            }
        }

        public async Task SendRestPassword(string email, string link)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var schooleInfo = (await _schoolInfo.GetRelation()).First();
            var content = $@"<div style='width:100%;text-align:left;'>
                    <h1 style='width:100%;text-align:center;'>Hello {user.FirstName} {user.LastName}</h1>
                     <p style='font-size:14px;'>Welcome to {schooleInfo.SchoolName}!</p>
                    <p style='font-size:14px;'>To complete forget password, please click the button</p><br><br/>
                    <div style='width:100%;text-align:center'> <a href='{link}' style='text-decoration: none;color: white;padding: 10px 25px;border: none;border-radius: 4px;font-size: 20px;background-color: rgb(83, 136, 247);'>Click</a>
                    <p style='font-size:12px;margin-top:60px'>Thank you for your time. </p></div> </div>
                    ";
            await SendEmail(email ?? "", "Reset Password", Message(content), true);
        }

        
    }
}
