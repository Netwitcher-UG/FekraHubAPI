using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Mail;
namespace FekraHubAPI.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IRepository<Student> studentRepo;
        private readonly IRepository<StudentContract> studentContract;
        private readonly IConfiguration config;
        public EmailSender(IConfiguration config, UserManager<ApplicationUser> userManager,
            IRepository<Student> studentRepo, IRepository<StudentContract> studentContract , ApplicationDbContext context)
        {
            this.config = config;
            this.userManager = userManager;
            this.studentRepo = studentRepo;
            this.studentContract = studentContract;
            this.context = context;
        }

        private Task SendEmail(List<string> toEmail, string subject, string body, bool isBodyHTML, byte[]? pdf = null, string? pdfName = null)
        {

            string MailServer = config["EmailSenderSettings:Server"] ?? "smtp.ionos.de";
            int Port = int.Parse(config["EmailSenderSettings:Port"] ?? "587");
            string FromEmail = config["EmailSenderSettings:FromEmail"] ?? "info@fekrahub.com";
            string Password = config["EmailSenderSettings:Password"] ?? "Fekra@hub2024";
            var client = new SmtpClient(MailServer, Port)
            {
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl = true,
            };
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHTML
            };

            foreach (var email in toEmail)
            {
                mailMessage.To.Add(email);
            }
            mailMessage.Headers.Add("Disposition-Notification-To", FromEmail);
            if(pdf != null)
            {
                Attachment pdfAttachment = new Attachment(new MemoryStream(pdf), pdfName ?? "pdf" , "application/pdf");
                mailMessage.Attachments.Add(pdfAttachment);
            }
            
            return client.SendMailAsync(mailMessage);
        }
        private string Message(string contestHtml)
        {
            string ConstantsMessage = @$"<div class='container' style='width: 100%;background-color: rgb(242, 242, 242);text-align: center;padding: 20px 0;margin: 0;'>
                <div class='message' style=' width: 300px;margin: 0 auto;'>
                    <header style='width: 90%;margin: 0 auto;display: flex;justify-content: space-between;align-items: center;'>
                        <h3>Fekra Hup</h3>
                        <img src='home2.png' alt='' style='width: 40px;'>
                    </header>
                    <div class='content' style='background-color: rgb(255, 255, 255);padding: 20px;text-align: left;'>
                    {contestHtml}
                    </div>
                    <footer>
                        <p>copyright fekraHup</p>
                    </footer>
                </div>
            </div>";
            return ConstantsMessage;
        }
        public async Task<IActionResult> SendConfirmationEmail(ApplicationUser user)
        {
            string FekraHupUrl = config["EmailSenderSettings:Url"] ?? "https://localhost:7288"; //change this later
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{FekraHupUrl}/NewUser/confirm?ID={user.Id}&Token={token}";
            var content = $@"<div style='width:100%;text-align:center;'>
                            <h1>Hello {user.UserName}</h1>
                             <p>Welcome to FekraHup!, Thank you For Confirming your Account,</p>
                             <p>The activation button is valid for <b> 7 Days</b>. Please activate the email before this period expires</p>
                            <p>To complete the confirmation, please click the confirm button</p><br><br/>
                            <a href='{confirmationLink}' style='text-decoration: none;padding: 10px 20px;border-radius: 5px;cursor: pointer;background-color: #3b73fe;color: white;font-size:24px;'>
                            Confirm Now</a> </div>
                            ";
            try
            {
                await SendEmail([user.Email ?? ""], "Please Confirm Your Email", Message(content), true);
                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return new BadRequestObjectResult($"Error sending email: {ex.Message}");
            }
        }

        public async Task<IActionResult> SendContractEmail(int studentId,string pdfName)
        {
            var student = await studentRepo.GetById(studentId);
            var parent = await userManager.FindByIdAsync(student.ParentID ?? "");
            if (student == null || parent == null)
            {
                return new BadRequestObjectResult("Something seems wrong. Please re-register your child or contact us");
            }
            var contracts = await studentContract.GetAll();
            byte[] contract = contracts.Where(x => x.StudentID == studentId).Select(x => x.File).First();
            var content = @$"<div style='width:100%;text-align:center;'>
                        <h1>Hello {parent.FirstName} {parent.LastName}</h1>
                         <p>Your son {student.FirstName} {student.LastName} has been registered successfully.</p>
                         <p>A copy of the contract has been sent to you. </p>
                        <p>For more information contact us</p>
                        <p>Thank you for your time. </p>
                     </div>";
            try
            {
                await SendEmail([parent.Email ?? ""], "", Message(content), true, contract, pdfName);
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
            var AdminId = context.UserRoles.Where(x => x.RoleId == "1").Select(x => x.UserId).FirstOrDefault();
            var adminEmail = userManager.Users.Where(x => x.Id == AdminId).Select(x => x.Email).FirstOrDefault();
            var content = @$"<div style='width:100%;text-align:center;'>
                        <h1>Hello Manager</h1>
                         <p>A new family has been registered</p>
                         <ul>
                            <li>
                            {user.FirstName} {user.LastName}
                            </li>
                            <li>
                            {user.StreetNr} {user.City} {user.ZipCode}
                            </li>
                            <li>
                            {user.Email}
                            </li>
                            <li>
                            {user.PhoneNumber} / {user.EmergencyPhoneNumber}
                            </li>
                        </ul>
                        <p>Thank you for your time. </p>
                     </div>";
            await SendEmail([adminEmail], "", Message(content), true);
        }

        public async Task SendToAllNewEvent()
        {
            var users = await userManager.Users.ToListAsync();
            foreach(var user in users)
            {
                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1>Hello {user.Name}</h1>
                         <p>A new event has been added</p>
                        <p>Thank you for your time. </p>
                     </div>";
                await SendEmail([user.Email], "", Message(content), true);
            }
            
        }

        public async Task SendToParentsNewFiles()
        {
            var ParentsId = await context.UserRoles
                            .Where(x => x.RoleId == "3")
                            .Select(x => x.UserId)
                            .ToListAsync();
            var Users = await context.Users
                         .Where(user => ParentsId.Contains(user.Id))
                         .ToListAsync();
            foreach(var user in Users)
            {
                var content = @$"<div style='width:100%;text-align:center;'>
                        <h1>Hello </h1>
                         <p>New files has been added</p>
                        <p>Thank you for your time. </p>
                     </div>";
                await SendEmail([user.Email], "", Message(content), true);
            }
            
        }

        public async Task SendToSecretaryNewReportsForStudents()
        {
            var ParentsId = await context.UserRoles
                            .Where(x => x.RoleId == "2")
                            .Select(x => x.UserId)
                            .ToListAsync();
            var emails = await context.Users
                         .Where(user => ParentsId.Contains(user.Id))
                         .Select(user => user.Email)
                         .ToListAsync();
            var content = @$"<div style='width:100%;text-align:center;'>
                        <h1>Hello</h1>
                         <p>New reports has been added</p>
                        <p>Thank you for your time. </p>
                     </div>";
            await SendEmail(emails, "", Message(content), true);
        }

        public async Task SendToParentsNewReportsForStudents()
        {
            var ParentsId = await context.UserRoles
                            .Where(x => x.RoleId == "3")
                            .Select(x => x.UserId)
                            .ToListAsync();
            var emails = await context.Users
                         .Where(user => ParentsId.Contains(user.Id))
                         .Select(user => user.Email)
                         .ToListAsync();
            var content = @$"<div style='width:100%;text-align:center;'>
                        <h1>Hello Manager</h1>
                         <p>A new report has been added</p>
                        <p>Thank you for your time. </p>
                     </div>";
            await SendEmail(emails, "", Message(content), true);
        }
    }
}
