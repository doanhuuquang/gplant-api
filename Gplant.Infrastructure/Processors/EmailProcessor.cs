using Gplant.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Gplant.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using Gplant.Domain.DTOs.Requests.Email;

namespace Gplant.Infrastructure.Processors
{
    public class EmailProcessor(IOptions<EmailOptions> emailOptions) : IEmailProcessor
    {
        public async Task SendEmail(EmailRequest emailRequest)
        {
            var email = emailOptions.Value.Email;
            var password = emailOptions.Value.Password;
            var host = emailOptions.Value.Host;
            var port = int.Parse(emailOptions.Value.Port);

            var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email, password)
            };

            var message = new MailMessage(email, emailRequest.Receptor, emailRequest.Subject, emailRequest.Body);

            await smtpClient.SendMailAsync(message);
        }
    }
}
