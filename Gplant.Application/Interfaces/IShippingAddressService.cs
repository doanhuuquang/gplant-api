using Gplant.Domain.DTOs.Requests.ShippingAddress;
using Gplant.Domain.DTOs.Responses;

namespace Gplant.Application.Interfaces
{
    public interface IShippingAddressService
    {
        Task<List<ShippingAddressResponse>> GetShippingAddressesByUserIdAsync(Guid userId);
        Task AddShippingAddressAsync(Guid userId, AddShippingAddressRequest request);
        Task UpdateShippingAddressAsync(Guid id, UpdateShippingAddressRequest request);
        Task DeleteShippingAddressAsync(Guid shippingAddressId);
    }
}
