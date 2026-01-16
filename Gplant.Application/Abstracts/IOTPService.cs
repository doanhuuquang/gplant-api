using Gplant.Domain.DTOs.Requests;

namespace Gplant.Application.Abstracts
{
    public interface IOTPService
    {
        public Task SendOTPToEmailAsync(SendOTPToEmailRequest sendOTPToEmailRequest);
        public Task<string?> VerifyOTPAsync(VerifyOTPRequest verifyOTPRequest);
    }
}
