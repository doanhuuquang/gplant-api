using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Cart;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gplant.API.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartController(ICartService cartService) : ControllerBase
    {
        /// <summary>
        /// Get my cart
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetCurrentUserId();
            var cart = await cartService.GetMyCartAsync(userId);

            var response = new SuccessResponse<CartResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get cart successful.",
                Data: cart,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(AddToCartRequest request)
        {
            var userId = GetCurrentUserId();
            var cart = await cartService.AddToCartAsync(userId, request);

            var response = new SuccessResponse<CartResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Item added to cart successfully.",
                Data: cart,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("items/{cartItemId:guid}")]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, UpdateCartItemRequest request)
        {
            var userId = GetCurrentUserId();
            var cart = await cartService.UpdateCartItemAsync(userId, cartItemId, request);

            var response = new SuccessResponse<CartResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Cart item updated successfully.",
                Data: cart,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("items/{cartItemId:guid}")]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var userId = GetCurrentUserId();
            await cartService.RemoveFromCartAsync(userId, cartItemId);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Item removed from cart successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Clear cart
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            await cartService.ClearCartAsync(userId);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Cart cleared successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get cart item count
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetCurrentUserId();
            var count = await cartService.GetCartItemCountAsync(userId);

            var response = new SuccessResponse<object>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get cart item count successful.",
                Data: new { Count = count },
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Sync cart prices (update prices from current variant/sale prices)
        /// </summary>
        [HttpPost("sync-prices")]
        public async Task<IActionResult> SyncCartPrices()
        {
            var userId = GetCurrentUserId();
            await cartService.SyncCartPricesAsync(userId);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Cart prices synced successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User not authenticated");

            return userId;
        }
    }
}