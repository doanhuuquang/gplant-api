using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class OrderRepository(ApplicationDbContext context) : IOrderRepository
    {
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<PagedResult<Order>> GetOrdersAsync(OrderFilterRequest filter, Guid? userId = null)
        {
            var query = context.Orders.AsQueryable();

            // Filter by user
            if (userId.HasValue)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }

            // Filter by status
            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.Status == filter.Status.Value);
            }

            // Filter by payment status
            if (filter.PaymentStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);
            }

            // Filter by date range
            if (filter.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAtUtc >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAtUtc <= filter.ToDate.Value);
            }

            // Search by order number or shipping info
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(searchTerm) ||
                    o.ShippingName.ToLower().Contains(searchTerm) ||
                    o.ShippingPhone.Contains(searchTerm));
            }

            // Sort by created date descending
            query = query.OrderByDescending(o => o.CreatedAtUtc);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<List<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAtUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetOrderCountTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await context.Orders
                .CountAsync(o => o.CreatedAtUtc >= today);
        }

        public async Task CreateAsync(Order order)
        {
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            order.UpdatedAtUtc = DateTime.UtcNow;
            context.Orders.Update(order);
            await context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetOrdersWithPaymentTimeoutAsync(DateTime cutoffTime)
        {
            return await context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.AwaitingPayment 
                         && o.PaymentAttemptedAtUtc < cutoffTime)
                .ToListAsync();
        }

        public async Task<bool> HasOrdersByUserIdAsync(Guid userId)
        {
            return await context.Orders.AnyAsync(o => o.UserId == userId);
        }
    }
}