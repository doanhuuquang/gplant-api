using Gplant.Domain.DTOs.Requests.OTP;

namespace Gplant.Application.Interfaces
{
    public interface IOTPService
    {
        public Task SendOTPToEmailAsync(SendOTPToEmailRequest sendOTPToEmailRequest);
        public Task<string?> VerifyOTPAsync(VerifyOTPRequest verifyOTPRequest);
    }
}
