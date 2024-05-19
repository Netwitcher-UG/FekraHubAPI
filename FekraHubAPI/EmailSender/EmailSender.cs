using FekraHubAPI.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        
        private Task EmaiNotification(string toEmail, string subject, string body, bool isBodyHTML)
        {
            
            string MailServer = config["EmailSenderSettings:Server"] ?? "smtp.ionos.de";
            int Port = int.Parse(config["EmailSenderSettings:Port"] ?? "587") ;
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
            return client.SendMailAsync(mailMessage);
        }

        public IConfiguration GetConfig()
        {
            return config;
        }

        public async Task SendConfirmationEmail(ApplicationUser user)
        {
            if(user != null && user.Email != null && user.UserName != null)
            {
                string FekraHupUrl = config["EmailSenderSettings:Url"] ?? "https://localhost:7288"; //change this later
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{FekraHupUrl}/NewUser/confirm?ID={user.Id}&Token={token}";
                var message = $@"<div style='width:100%;text-align:center;'>
                            <h1>Hi {user.UserName}</h1>
                             <p>Welcome to FekraHup!, Thank you For Confirming your Account,</p>
                            <p>To complete the confirmation, please click the confirm button</p><br><br/>
                            <a href='{confirmationLink}' style='text-decoration: none;padding: 10px 20px;border-radius: 5px;cursor: pointer;background-color: #3b73fe;color: white;font-size:24px;'>
                            Confirm Now</a> </div>
                            <br><br/><br><br/><br><br/>
                            <h3>Thanks<br>
                            FekraHub Team</h3>";

                await EmaiNotification(user.Email, "Please Confirm Your Email", message, true);

            }

        }
    }
}
