using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace WebApplication1.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("858a30c593ab86", "9a47eecf0f4f26"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("admin@netAuction.ro", "NetAuction Team"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            return client.SendMailAsync(mailMessage);
        }
    }
}