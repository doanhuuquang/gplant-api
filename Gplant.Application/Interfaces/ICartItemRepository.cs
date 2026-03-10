using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ICartItemRepository
    {
        Task<CartItem?> GetByIdAsync(Guid id);
        Task<List<CartItem>> GetByCartIdAsync(Guid cartId);
        Task<CartItem?> GetByCartAndVariantAsync(Guid cartId, Guid plantVariantId);
        Task CreateAsync(CartItem item);
        Task UpdateAsync(CartItem item);
        Task DeleteAsync(CartItem item);
        Task DeleteByCartIdAsync(Guid cartId);
    }
}