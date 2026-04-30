using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Enums;

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
        Task<List<Order>> GetOrdersWithPaymentTimeoutAsync(DateTimeOffset cutoffTime);
        Task<bool> HasOrdersByUserIdAsync(Guid userId);
        Task<List<Order>> GetOrdersTimeoutAsync(PaymentMethod paymentMethod, DateTimeOffset cutoff);
        Task<(int TodayOrderCount, decimal TodayRevenue, int PendingCount, int DeliveringCount)> GetDashboardStatsAsync();
        Task<List<Order>> GetRecentOrdersAsync(int limit = 5);
        Task<Dictionary<DateTime, decimal>> GetRevenueChartDataAsync(int days = 7);
    }
}