using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gplant.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        /// <summary>
        /// Get my orders
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderFilterRequest filter)
        {
            var userId = GetCurrentUserId();
            var orders = await orderService.GetMyOrdersAsync(userId, filter);

            var response = new SuccessResponse<PagedResult<OrderResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get my orders successful.",
                Data: orders,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get all orders (Admin/Manager only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterRequest filter)
        {
            var orders = await orderService.GetAllOrdersAsync(filter);

            var response = new SuccessResponse<PagedResult<OrderResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get all orders successful.",
                Data: orders,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var userId = GetCurrentUserId();
            var order = await orderService.GetByIdAsync(id, userId);

            var response = new SuccessResponse<OrderResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get order successful.",
                Data: order,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        [HttpGet("number/{orderNumber}")]
        public async Task<IActionResult> GetOrderByNumber(string orderNumber)
        {
            var userId = GetCurrentUserId();
            var order = await orderService.GetByOrderNumberAsync(orderNumber, userId);

            var response = new SuccessResponse<OrderResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get order successful.",
                Data: order,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Create new order (Checkout)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateOrderResponse>> CreateOrder(CreateOrderRequest request)
        {
            var userId = GetCurrentUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var order = await orderService.CreateOrderAsync(userId, request, ipAddress);

            var response = new SuccessResponse<CreateOrderResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create order successful.",
                Data: order,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Update order status (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, UpdateOrderStatusRequest request)
        {
            await orderService.UpdateOrderStatusAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Order status updated successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id, CancelOrderRequest request)
        {
            var userId = GetCurrentUserId();
            await orderService.CancelOrderAsync(id, userId, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Order cancelled successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get order statistics (Admin/Manager only)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetOrderStats()
        {
            var stats = await orderService.GetOrderStatsAsync();

            var response = new SuccessResponse<Dictionary<string, object>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get order stats successful.",
                Data: stats,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Payment callback (for external payment gateways)
        /// </summary>
        [HttpGet("payment/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback()
        {
            var queryParams = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var result = await orderService.ProcessPaymentCallbackAsync(queryParams);
            
            if (result.Success)
            {
                return Redirect($"/order-success?orderNumber={result.OrderNumber}");
            }
            
            return Redirect($"/order-failed?message={result.Message}");
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