using Gplant.API.ApiResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Gplant.Application.Abstracts;
using Gplant.Domain.DTOs.Requests;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.API.Controllers
{
    [Route("api/otp")]
    [ApiController]
    public class OTPController(IOTPService otpService) : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendOTPToEmail(SendOTPToEmailRequest sendOTPToEmailRequest)
        {
            await otpService.SendOTPToEmailAsync(sendOTPToEmailRequest);

            var response = new SuccessResponse<object?>(
                StatusCode: 200,
                Message: "The OTP code has been successfully sent to your email.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOTP(VerifyOTPRequest verifyOTPRequest)
        {
            var resetPasswordToken = await otpService.VerifyOTPAsync(verifyOTPRequest);

            var response = new SuccessResponse<VerifyOTPResponse?>(
                StatusCode: 200,
                Message: "OTP verified successfully.",
                Data: new VerifyOTPResponse { ResetToken = resetPasswordToken ?? "" },
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}
