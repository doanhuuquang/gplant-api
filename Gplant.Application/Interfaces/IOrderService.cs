using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.DTOs.Responses;

namespace Gplant.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponse> GetByIdAsync(Guid id, Guid? userId = null);
        Task<OrderResponse> GetByOrderNumberAsync(string orderNumber, Guid? userId = null);
        Task<PagedResult<OrderResponse>> GetMyOrdersAsync(Guid userId, OrderFilterRequest filter);
        Task<OrderPagedResult> GetAllOrdersAsync(OrderFilterRequest filter);
        Task<CreateOrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request, string? ipAddress = null);
        Task UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
        Task CancelOrderAsync(Guid orderId, Guid userId, CancelOrderRequest request);
        Task HandleVNPayIPNAsync(string orderCode, string responseCode, string transactionId); // THÊM
        Task<Dictionary<string, object>> GetOrderStatsAsync();
    }
}