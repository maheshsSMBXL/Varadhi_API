using Varadhi.Models;

namespace Varadhi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailData emailData);
    }
}
