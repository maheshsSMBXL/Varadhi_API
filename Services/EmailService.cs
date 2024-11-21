using System.Net.Mail;
using System.Net;
using Varadhi.Models;

namespace Varadhi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(EmailData emailData)
        {
            var smtpClient = new SmtpClient(_configuration["Email:SMTPServer"])
            {
                Port = 587,
                Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailData.From),
                Subject = emailData.Subject,
                Body = emailData.Body,
                IsBodyHtml = emailData.Type == "html",
            };

            mailMessage.To.Add(emailData.To);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
