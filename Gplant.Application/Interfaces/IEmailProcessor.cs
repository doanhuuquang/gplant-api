using Gplant.Domain.DTOs.Requests.Email;

namespace Gplant.Application.Interfaces
{
    public interface IEmailProcessor
    {
        public Task SendEmail(EmailRequest emailRequest);
    }
}
