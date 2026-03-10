using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(Guid id);
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task CreateAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Cart cart);
    }
}