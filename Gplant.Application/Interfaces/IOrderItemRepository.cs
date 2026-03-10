using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<OrderItem?> GetByIdAsync(Guid id);
        Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task CreateAsync(OrderItem item);
        Task CreateBulkAsync(List<OrderItem> items);
        Task UpdateAsync(OrderItem item);
    }
}