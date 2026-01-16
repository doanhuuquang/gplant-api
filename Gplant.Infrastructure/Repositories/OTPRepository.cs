using Microsoft.EntityFrameworkCore;
using Gplant.Application.Abstracts;
using Gplant.Domain.Entities;

namespace Gplant.Infrastructure.Repositories
{
    public class OTPRepository(ApplicationDbContext applicationDbContext, IOTPGenerator otpGenerator) : IOTPRepository
    {
        public async Task<OTP?> CreateOTPAsync(string email)
        {
            var code = otpGenerator.Generate();

            var otp = new OTP
            {
                Id = Guid.NewGuid(),
                Email = email,
                Code = code,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            await applicationDbContext.OTPs.AddAsync(otp);
            await applicationDbContext.SaveChangesAsync();

            return otp;
        }

        public async Task<OTP?> GetOTPAsync(string email)
        {
            var otp = await applicationDbContext.OTPs.Where(x => x.Email == email && !x.IsUsed && x.ExpiresAtUtc > DateTime.UtcNow)
                                                     .OrderByDescending(x => x.CreatedAtUtc)
                                                     .FirstOrDefaultAsync();
            return otp;
        }
        public async Task UpdateOTPAsync(OTP otp)
        {
            applicationDbContext.OTPs.Update(otp);
            await applicationDbContext.SaveChangesAsync();
        }
    }
}
