using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IShippingAddressRepository
    {
        
        public Task<ShippingAddress?> GetShippingAddressByIdAsync(Guid id);
        public Task<ShippingAddress?> GetPrimaryShippingAddressAsync();
        public Task<List<ShippingAddress>> GetShippingAddressesByUserIdAsync(Guid userId);
        public Task CreateShippingAddressAsync(ShippingAddress shippingAddress);
        public Task UpdateShippingAddressAsync(ShippingAddress shippingAddress);
        public Task DeleteShippingAddressAsync(ShippingAddress shippingAddress);
    }
}
