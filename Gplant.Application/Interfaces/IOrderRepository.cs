using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<PagedResult<Order>> GetOrdersAsync(OrderFilterRequest filter, Guid? userId = null);
        Task<List<Order>> GetUserOrdersAsync(Guid userId);
        Task<int> GetOrderCountTodayAsync();
        Task CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task<List<Order>> GetOrdersWithPaymentTimeoutAsync(DateTime cutoffTime);
        Task<bool> HasOrdersByUserIdAsync(Guid userId);
    }
}