using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class CartRepository(ApplicationDbContext context) : ICartRepository
    {
        public async Task<Cart?> GetByIdAsync(Guid id)
        {
            return await context.Carts.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            return await context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task CreateAsync(Cart cart)
        {
            await context.Carts.AddAsync(cart);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cart cart)
        {
            cart.UpdatedAtUtc = DateTime.UtcNow;
            context.Carts.Update(cart);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Cart cart)
        {
            context.Carts.Remove(cart);
            await context.SaveChangesAsync();
        }
    }
}