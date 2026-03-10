using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class OrderItemRepository(ApplicationDbContext context) : IOrderItemRepository
    {
        public async Task<OrderItem?> GetByIdAsync(Guid id)
        {
            return await context.OrderItems.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await context.OrderItems
                .Where(i => i.OrderId == orderId)
                .OrderBy(i => i.CreatedAtUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateAsync(OrderItem item)
        {
            await context.OrderItems.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task CreateBulkAsync(List<OrderItem> items)
        {
            await context.OrderItems.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItem item)
        {
            item.UpdatedAtUtc = DateTime.UtcNow;
            context.OrderItems.Update(item);
            await context.SaveChangesAsync();
        }
    }
}