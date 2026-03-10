using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class CartItemRepository(ApplicationDbContext context) : ICartItemRepository
    {
        public async Task<CartItem?> GetByIdAsync(Guid id)
        {
            return await context.CartItems.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<CartItem>> GetByCartIdAsync(Guid cartId)
        {
            return await context.CartItems
                .Where(i => i.CartId == cartId)
                .OrderBy(i => i.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<CartItem?> GetByCartAndVariantAsync(Guid cartId, Guid plantVariantId)
        {
            return await context.CartItems
                .FirstOrDefaultAsync(i => i.CartId == cartId && i.PlantVariantId == plantVariantId);
        }

        public async Task CreateAsync(CartItem item)
        {
            await context.CartItems.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CartItem item)
        {
            item.UpdatedAtUtc = DateTime.UtcNow;
            context.CartItems.Update(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CartItem item)
        {
            context.CartItems.Remove(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteByCartIdAsync(Guid cartId)
        {
            var items = await context.CartItems.Where(i => i.CartId == cartId).ToListAsync();
            context.CartItems.RemoveRange(items);
            await context.SaveChangesAsync();
        }
    }
}