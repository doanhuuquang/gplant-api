using Gplant.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions;
using Gplant.Domain.DTOs.Requests.OTP;
using Gplant.Domain.Exceptions.User;
using Gplant.Domain.DTOs.Requests.Email;
using Gplant.Domain.Exceptions.OTP;

namespace Gplant.Application.Services
{
    public class OTPService(UserManager<User> userManager, IOTPRepository otpRepository, IActionTokenRepository actionTokenRepository, IEmailProcessor emailProcessor, IAuthTokenProcessor tokenProcessor) : IOTPService
    {
        public async Task SendOTPToEmailAsync(SendOTPToEmailRequest sendOTPToEmailRequest)
        {
            var userExists = await userManager.FindByEmailAsync(sendOTPToEmailRequest.Email);

            if (userExists is null) throw new UserNotExistsException(email: sendOTPToEmailRequest.Email);
            else
            {
                var otp = await otpRepository.CreateOTPAsync(sendOTPToEmailRequest.Email) ?? throw new OTPException("Failed to generate OTP. Please try again");

                var emailRequest = new EmailRequest
                {
                    Receptor = sendOTPToEmailRequest.Email,
                    Subject = "Reset password",
                    Body = $"Hello {userExists.FirstName},\n\nYour OTP is: {otp.Code}\n\nBest regards,\nQuizzen Team"
                };
                await emailProcessor.SendEmail(emailRequest);
            }
        }

        public async Task VerifyOTPAsync(VerifyOTPRequest verifyOTPRequest)
        {
            var otp = await otpRepository.GetOTPAsync(verifyOTPRequest.Email) ?? throw new OTPException("Invalid or expired OTP. Please request a new one.");

            if (otp.Code != verifyOTPRequest.Code) throw new OTPException("Invalid OTP. Please try again.");

            otp.IsUsed = true;

            var user = await userManager.FindByEmailAsync(verifyOTPRequest.Email) ?? throw new UserNotExistsException(email: verifyOTPRequest.Email);
            var resetPasswordToken = await actionTokenRepository.CreateActionTokenAsync(user.Id) ?? throw new OTPException("Failed to create reset password token. Please try again.");

            await otpRepository.UpdateOTPAsync(otp);

            tokenProcessor.WriteAuthTokenAsHttpOnlyCookie("RESET_PASSWORD_TOKEN", resetPasswordToken.Token, DateTime.UtcNow.AddMinutes(15));
        }
    }
}
