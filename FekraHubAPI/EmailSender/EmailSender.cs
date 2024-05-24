using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mail;
namespace FekraHubAPI.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        public EmailSender(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            this.config = config;
            this.userManager = userManager;
        }

        private Task SendEmail(string toEmail, string subject, string body, bool isBodyHTML)
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
            MailMessage mailMessage = new MailMessage(FromEmail, toEmail, subject, body)
            {
                IsBodyHtml = isBodyHTML
            };
            mailMessage.Headers.Add("Disposition-Notification-To", FromEmail);
            return client.SendMailAsync(mailMessage);
        }

        public async Task<IActionResult> SendConfirmationEmail(ApplicationUser user)
        {
            string FekraHupUrl = config["EmailSenderSettings:Url"] ?? "https://localhost:7288"; //change this later
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{FekraHupUrl}/NewUser/confirm?ID={user.Id}&Token={token}";
            var message = $@"<div style='width:100%;text-align:center;'>
                            <h1>Hi {user.UserName}</h1>
                             <p>Welcome to FekraHup!, Thank you For Confirming your Account,</p>
                             <p>The activation button is valid for <b> 7 Days</b>. Please activate the email before this period expires</p>
                            <p>To complete the confirmation, please click the confirm button</p><br><br/>
                            <a href='{confirmationLink}' style='text-decoration: none;padding: 10px 20px;border-radius: 5px;cursor: pointer;background-color: #3b73fe;color: white;font-size:24px;'>
                            Confirm Now</a> </div>
                            <br><br/><br><br/><br><br/>
                            <h3>Thanks<br>
                            FekraHub Team</h3>";
            try
            {
                await SendEmail(user.Email ?? "", "Please Confirm Your Email", message, true);
                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return new BadRequestObjectResult($"Error sending email: {ex.Message}");
            }
        }
    }
}
