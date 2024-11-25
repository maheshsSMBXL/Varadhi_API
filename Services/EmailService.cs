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
                Port = int.Parse(_configuration["Email:Port"]),
                Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            ),
                EnableSsl = bool.Parse(_configuration["Email:EnableSsl"]),
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailData.From),
                Subject = emailData.Subject,
                Body = emailData.Body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(emailData.To);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw; // Optionally re-throw the exception to handle it further up the call stack
            }
        }
    }
}
