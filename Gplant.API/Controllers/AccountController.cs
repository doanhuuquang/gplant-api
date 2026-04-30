using Gplant.API.ApiResponse;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Gplant.Domain.DTOs.Requests.Auth;

namespace Gplant.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(IAccountService accountService, SignInManager<User> signInManager, LinkGenerator linkGenerator) : ControllerBase
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="registerRequest"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            await accountService.RegisterAsync(registerRequest);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Register successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            await accountService.LoginAsync(loginRequest);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Login successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = HttpContext.Request.Cookies["REFRESH_TOKEN"];

            await accountService.LogoutAsync(refreshToken);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Logout successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = HttpContext.Request.Cookies["REFRESH_TOKEN"];

            await accountService.RefreshTokenAsync(refreshToken);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Refresh token successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("recover-username")]
        public async Task<IActionResult> RecoverUsername(RecoverUsernameRequest recoverUsernameRequest)
        {
            await accountService.RecoverUsernameAsync(recoverUsernameRequest);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Recover username successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var resetPasswordToken = HttpContext.Request.Cookies["RESET_PASSWORD_TOKEN"];
            await accountService.ResetPasswordAsync(resetPasswordRequest, resetPasswordToken);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Reset password successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet("login/google")]
        public IActionResult LoginGoogle([FromQuery] string? returnUrl)
        {
            var redirectUrl = linkGenerator.GetPathByName(HttpContext, "GoogleLoginCallback");

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                redirectUrl
            );

            properties.Items["returnUrl"] = returnUrl ?? "/";

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("login/google/callback", Name = "GoogleLoginCallback")]
        public async Task<IActionResult> LoginGoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded) return Unauthorized();

            var returnUrl = result.Properties?.Items["returnUrl"] ?? "/";

            await accountService.LoginWithGoogleAsync(result.Principal);

            return Redirect(returnUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet("login/facebook")]
        public IActionResult LoginFacebook([FromQuery] string? returnUrl)
        {
            var redirectUrl = linkGenerator.GetPathByName(HttpContext, "FacebookLoginCallback");

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                FacebookDefaults.AuthenticationScheme,
                redirectUrl
            );

            properties.Items["returnUrl"] = returnUrl ?? "/";

            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("login/facebook/callback", Name = "FacebookLoginCallback")]
        public async Task<IActionResult> LoginFacebookCallback()
        {
            var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

            if (!result.Succeeded) return Unauthorized();

            var returnUrl = result.Properties?.Items["returnUrl"] ?? "/";

            await accountService.LoginWithFacebookAsync(result.Principal);

            return Redirect(returnUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet("login/microsoft")]
        public IActionResult LoginMicrosoft([FromQuery] string? returnUrl)
        {
            var redirectUrl = linkGenerator.GetPathByName(HttpContext, "MicrosoftLoginCallback");

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                MicrosoftAccountDefaults.AuthenticationScheme,
                redirectUrl
            );

            properties.Items["returnUrl"] = returnUrl ?? "/";

            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("login/microsoft/callback", Name = "MicrosoftLoginCallback")]
        public async Task<IActionResult> LoginMicrosoftCallback()
        {
            var result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

            if (!result.Succeeded) return Unauthorized();

            var returnUrl = result.Properties?.Items["returnUrl"] ?? "/";

            await accountService.LoginWithMicrosoftAsync(result.Principal);

            return Redirect(returnUrl);
        }
    }
}
