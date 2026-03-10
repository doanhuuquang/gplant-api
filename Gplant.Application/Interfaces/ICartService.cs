using Gplant.Domain.DTOs.Requests.Cart;
using Gplant.Domain.DTOs.Responses;

namespace Gplant.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartResponse> GetMyCartAsync(Guid userId);
        Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request);
        Task<CartResponse> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request);
        Task RemoveFromCartAsync(Guid userId, Guid cartItemId);
        Task ClearCartAsync(Guid userId);
        Task<int> GetCartItemCountAsync(Guid userId);
        Task SyncCartPricesAsync(Guid userId);
    }
}