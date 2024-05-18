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
                var confirmationLink = $"{FekraHupUrl}/NewUser/confirm?UserName={user.UserName}&Token={token}";
                await EmaiNotification(user.Email, "Please Confirm Your Email", $"<h3>Please confirm your account by click</h3> <a href='{confirmationLink}' style='text-decoration: none;padding: 8px 15px;border-radius: 5px;cursor: pointer;background-color: #3b73fe;color: white;'>Confirm Now</a>", true);

            }

        }
    }
}
