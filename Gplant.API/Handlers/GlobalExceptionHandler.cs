using Gplant.API.ApiResponse;
using Gplant.Domain.Exceptions.ActionToken;
using Gplant.Domain.Exceptions.Auth;
using Gplant.Domain.Exceptions.Banner;
using Gplant.Domain.Exceptions.CareInstruction;
using Gplant.Domain.Exceptions.Cart;
using Gplant.Domain.Exceptions.Category;
using Gplant.Domain.Exceptions.Inventory;
using Gplant.Domain.Exceptions.LightningSale;
using Gplant.Domain.Exceptions.Order;
using Gplant.Domain.Exceptions.OTP;
using Gplant.Domain.Exceptions.Plant;
using Gplant.Domain.Exceptions.PlantVariant;
using Gplant.Domain.Exceptions.User;
using Gplant.Domain.Exceptions.Media;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Gplant.API.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, errorCode) = exception switch
            {
                LoginNotFoundAccountException   => (HttpStatusCode.NotFound, "LoginNotFoundAccount"),
                LoginIncorrectPasswordException => (HttpStatusCode.Unauthorized, "LoginIncorrectPassword"),
                UserAlreadyExistsException      => (HttpStatusCode.Conflict, "UserAlreadyExists"),
                RegistrationFailedException     => (HttpStatusCode.BadRequest, "RegistrationFailed"),
                RefreshTokenException           => (HttpStatusCode.Unauthorized, "RefreshTokenError"),
                UserNotExistsException          => (HttpStatusCode.NotFound, "UserNotExists"),

                OTPException            => (HttpStatusCode.BadRequest, "OTPError"),
                ActionTokenException    => (HttpStatusCode.BadRequest, "ActionTokenError"),
                ResetPasswordException  => (HttpStatusCode.BadRequest, "ResetPasswordError"),

                CategoryNotFoundException   => (HttpStatusCode.NotFound, "CategoryNotFound"),
                CategoryException           => (HttpStatusCode.BadRequest, "CategoryError"),

                PlantNotFoundException  => (HttpStatusCode.NotFound, "PlantNotFound"),
                PlantVariantException   => (HttpStatusCode.BadRequest, "PlantVariantError"),
                PlantException          => (HttpStatusCode.BadRequest, "PlantError"),

                InsufficientStockException  => (HttpStatusCode.BadRequest, "InsufficientStock"),
                InventoryNotFoundException  => (HttpStatusCode.NotFound, "InventoryNotFound"),
                InventoryException          => (HttpStatusCode.BadRequest, "InventoryError"),

                CartItemNotFoundException   => (HttpStatusCode.NotFound, "CartItemNotFound"),
                CartException               => (HttpStatusCode.BadRequest, "CartError"),

                OrderNotFoundException      => (HttpStatusCode.NotFound, "OrderNotFound"),
                InvalidOrderStatusException => (HttpStatusCode.BadRequest, "InvalidOrderStatus"),
                OrderException              => (HttpStatusCode.BadRequest, "OrderError"),

                LightningSaleNotFoundException  => (HttpStatusCode.NotFound, "LightningSaleNotFound"),
                LightningSaleException          => (HttpStatusCode.BadRequest, "LightningSaleError"),

                BannerNotFoundException     => (HttpStatusCode.NotFound, "BannerNotFound"),
                CreateBannerException       => (HttpStatusCode.BadRequest, "CreateBannerError"),
                UpdateBannerException       => (HttpStatusCode.BadRequest, "UpdateBannerError"),

                CareInstructionException => (HttpStatusCode.BadRequest, "CareInstructionError"),

                ExternalLoginProviderException => (HttpStatusCode.BadRequest, "ExternalLoginProviderError"),

                MediaNotFoundException          => (HttpStatusCode.NotFound, "MediaNotFound"),
                InvalidFileException            => (HttpStatusCode.BadRequest, "InvalidFile"),
                MediaInUseException             => (HttpStatusCode.BadRequest, "MediaInUse"),

                _ => (HttpStatusCode.InternalServerError, "InternalServerError")

            };

            logger.LogError(exception, "An error occurred while processing the request");

            var response = new ErrorResponse(
                StatusCode: (int)statusCode,
                Error:      errorCode,
                Message:    exception.Message,
                Timestamp:  DateTime.UtcNow
            );

            httpContext.Response.StatusCode     = (int)statusCode;
            httpContext.Response.ContentType    = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
