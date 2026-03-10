using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IOTPRepository
    {
        public Task<OTP?> CreateOTPAsync(string email);
        public Task<OTP?> GetOTPAsync(string email);
        public Task UpdateOTPAsync(OTP otp);
    }
}
