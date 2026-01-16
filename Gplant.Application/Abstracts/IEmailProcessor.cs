using Gplant.Domain.DTOs.Requests;

namespace Gplant.Application.Abstracts
{
    public interface IEmailProcessor
    {
        public Task SendEmail(EmailRequest emailRequest);
    }
}
