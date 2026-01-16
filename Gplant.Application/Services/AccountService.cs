using Gplant.Application.Abstracts;
using Microsoft.AspNetCore.Identity;
using Gplant.Domain.DTOs.Requests;
using Gplant.Domain.Entities;
using Gplant.Domain.enums;
using Gplant.Domain.Exceptions;
using System.Security.Claims;

namespace Gplant.Application.Services
{
    public class AccountService(IAuthTokenProcessor authTokenProcessor, IEmailProcessor emailProcessor, UserManager<User> userManager, IUserRepository userRepository, IActionTokenRepository actionTokenRepository) : IAccountService
    {
        public async Task RegisterAsync(RegisterRequest registerRequest)
        {
            var userExists = await userManager.FindByEmailAsync(registerRequest.Email);

            if (userExists is not null) throw new UserAlreadyExistsException(email: registerRequest.Email);

            var user = User.Create(registerRequest.Email);
            user.FirstName = registerRequest.FirstName;
            user.LastName = registerRequest.LastName;

            var result = await userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded) throw new RegistrationFailedException(result.Errors.Select(x => x.Description));
        }

        public async Task LoginAsync(LoginRequest loginRequest)
        {
            var user = await userManager.FindByEmailAsync(loginRequest.Email) ?? throw new LoginNotFoundAccountException(email: loginRequest.Email);

            if (await userManager.CheckPasswordAsync(user, loginRequest.Password) == false) throw new LoginIncorrectPasswordException();

            var (jwtToken, expirationDateInUtc) = authTokenProcessor.GenerateJwtToken(user);
            var refreshTokenValue = authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationInUtc = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationInUtc;

            await userManager.UpdateAsync(user);

            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshTokenValue, refreshTokenExpirationInUtc);
        }

        public async Task LogoutAsync(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new RefreshTokenException("Refresh token is missing!");

            var user = await userRepository.GetUserByRefreshTokenAsync(refreshToken) ?? throw new RefreshTokenException("Invalid refresh token!");

            if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow) throw new RefreshTokenException("Refresh token is expired!");

            user.RefreshToken = null;
            user.RefreshTokenExpiresAtUtc = null;

            await userManager.UpdateAsync(user);

            authTokenProcessor.DeleteAuthTokenCookie("ACCESS_TOKEN");
            authTokenProcessor.DeleteAuthTokenCookie("REFRESH_TOKEN");
        }

        public async Task RefreshTokenAsync(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new RefreshTokenException("Refresh token is missing!");

            var user = await userRepository.GetUserByRefreshTokenAsync(refreshToken) ?? throw new RefreshTokenException("Invalid refresh token!");

            if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow) throw new RefreshTokenException("Refresh token is expired!");

            var (jwtToken, expirationDateInUtc) = authTokenProcessor.GenerateJwtToken(user);
            var refreshTokenValue = authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationInUtc = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationInUtc;

            await userManager.UpdateAsync(user);

            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshTokenValue, refreshTokenExpirationInUtc);
        }

        public async Task LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null) throw new ExternalLoginProviderException("Google", "ClaimsPrincipal is null");

            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email) ?? throw new ExternalLoginProviderException("Google", "Email not found");

            var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new ExternalLoginProviderException("Google", "Provider key not found");

            var user = await userManager.FindByLoginAsync("Google", providerKey);

            if (user == null)
            {
                user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    var newUser = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                        LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                        ProfilePictureUrl = claimsPrincipal.FindFirst("image")?.Value ?? string.Empty,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(newUser);

                    if (!result.Succeeded) throw new ExternalLoginProviderException("Google", $"Unable to creat user: {string.Join(", ", result.Errors.Select(x => x.Description))}");

                    user = newUser;
                }

                var info = new UserLoginInfo("Google", providerKey, "Google");

                var loginResult = await userManager.AddLoginAsync(user, info);

                if (!loginResult.Succeeded) throw new ExternalLoginProviderException("Google", $"Unable to login the user: {string.Join(", ", loginResult.Errors.Select(x => x.Description))}");
            }

            var (jwtToken, expirationDateInUtc) = authTokenProcessor.GenerateJwtToken(user);
            var refreshTokenValue = authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationInUtc = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationInUtc;

            await userManager.UpdateAsync(user);

            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshTokenValue, refreshTokenExpirationInUtc);
        }

        public async Task LoginWithFacebookAsync(ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null) throw new ExternalLoginProviderException("Facebook", "ClaimsPrincipal is null");

            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email) ?? throw new ExternalLoginProviderException("Facebook", "Email not found");

            var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new ExternalLoginProviderException("Facebook", "Provider key not found");

            var user = await userManager.FindByLoginAsync("Facebook", providerKey);

            if (user == null)
            {
                user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    var newUser = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                        LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                        ProfilePictureUrl = $"https://graph.facebook.com/{providerKey}/picture?type=large",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(newUser);

                    if (!result.Succeeded) throw new ExternalLoginProviderException("Facebook", $"Unable to creat user: {string.Join(", ", result.Errors.Select(x => x.Description))}");

                    user = newUser;
                }

                var info = new UserLoginInfo("Facebook", providerKey, "Facebook");

                var loginResult = await userManager.AddLoginAsync(user, info);

                if (!loginResult.Succeeded) throw new ExternalLoginProviderException("Facebook", $"Unable to login the user: {string.Join(", ", loginResult.Errors.Select(x => x.Description))}");
            }

            var (jwtToken, expirationDateInUtc) = authTokenProcessor.GenerateJwtToken(user);
            var refreshTokenValue = authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationInUtc = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationInUtc;

            await userManager.UpdateAsync(user);

            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshTokenValue, refreshTokenExpirationInUtc);
        }

        public async Task LoginWithMicrosoftAsync(ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null) throw new ExternalLoginProviderException("Microsoft", "ClaimsPrincipal is null");

            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email) ?? throw new ExternalLoginProviderException("Microsoft", "Email not found");

            var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new ExternalLoginProviderException("Microsoft", "Provider key not found");

            var user = await userManager.FindByLoginAsync("Microsoft", providerKey);

            if (user == null)
            {
                user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    var newUser = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                        LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,

                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(newUser);

                    if (!result.Succeeded) throw new ExternalLoginProviderException("Microsoft", $"Unable to creat user: {string.Join(", ", result.Errors.Select(x => x.Description))}");

                    user = newUser;
                }

                var info = new UserLoginInfo("Microsoft", providerKey, "Microsoft");

                var loginResult = await userManager.AddLoginAsync(user, info);

                if (!loginResult.Succeeded) throw new ExternalLoginProviderException("Microsoft", $"Unable to login the user: {string.Join(", ", loginResult.Errors.Select(x => x.Description))}");
            }

            var (jwtToken, expirationDateInUtc) = authTokenProcessor.GenerateJwtToken(user);
            var refreshTokenValue = authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationInUtc = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationInUtc;

            await userManager.UpdateAsync(user);

            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
            authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshTokenValue, refreshTokenExpirationInUtc);
        }

        public async Task RecoverUsernameAsync(RecoverUsernameRequest recoverUsernameRequest)
        {
            var userExists = await userManager.FindByEmailAsync(recoverUsernameRequest.Email);

            if (userExists is null) throw new UserNotExistsException(email: recoverUsernameRequest.Email);
            else
            {
                var emailRequest = new EmailRequest
                {
                    Receptor = recoverUsernameRequest.Email,
                    subject = "Username Recovery",
                    body = $"Hello {userExists.FirstName},\n\nYour username is: {userExists.UserName}\n\nBest regards,\nQuizzen Team"
                };
                await emailProcessor.SendEmail(emailRequest);
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordRequest.Email) ?? throw new UserNotExistsException(email: resetPasswordRequest.Email);
            var resetPasswordToken = await actionTokenRepository.GetActionTokenAsync(user.Id, ActionTokenPurpose.ResetPassword) ?? throw new ActionTokenException("Invalid or expired reset password token.");

            if (resetPasswordToken.Token != resetPasswordRequest.ResetPasswordToken) throw new ActionTokenException("Invalid reset password token.");

            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, resetPasswordRequest.NewPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded) throw new ResetPasswordException(result.Errors.Select(x => x.Description));

            resetPasswordToken.IsUsed = true;
            await actionTokenRepository.UpdateActionTokenAsync(resetPasswordToken);
        }
    }
}
