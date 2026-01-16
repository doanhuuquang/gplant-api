using System.Net;
using Gplant.API.ApiResponse;
using Microsoft.AspNetCore.Diagnostics;
using Gplant.Domain.Exceptions;

namespace Gplant.API.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, errorCode) = exception switch
            {
                LoginNotFoundAccountException => (HttpStatusCode.NotFound, "LoginNotFoundAccount"),
                LoginIncorrectPasswordException => (HttpStatusCode.Unauthorized, "LoginIncorrectPassword"),
                UserAlreadyExistsException => (HttpStatusCode.Conflict, "UserAlreadyExists"),
                RegistrationFailedException => (HttpStatusCode.BadRequest, "RegistrationFailed"),
                RefreshTokenException => (HttpStatusCode.Unauthorized, "RefreshTokenError"),
                UserNotExistsException => (HttpStatusCode.NotFound, "UserNotExists"),
                OTPException => (HttpStatusCode.BadRequest, "OTPError"),
                ActionTokenException => (HttpStatusCode.BadRequest, "ActionTokenError"),
                ResetPasswordException => (HttpStatusCode.BadRequest, "ResetPasswordError"),
                _ => (HttpStatusCode.InternalServerError, "InternalServerError")

            };

            logger.LogError(exception, "An error occurred while processing the request");

            var response = new ErrorResponse(
                StatusCode: (int)statusCode,
                Error: errorCode,
                Message: exception.Message,
                Timestamp: DateTime.UtcNow
            );

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
