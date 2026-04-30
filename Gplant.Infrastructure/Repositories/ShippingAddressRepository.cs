using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class ShippingAddressRepository(ApplicationDbContext applicationDbContext) : IShippingAddressRepository
    {
        public async Task<ShippingAddress?> GetShippingAddressByIdAsync(Guid id)
        {
            return await applicationDbContext.ShippingAddresses
                                             .FirstOrDefaultAsync((shippingAddresses) => shippingAddresses.Id == id);
        }

        public async Task<List<ShippingAddress>> GetShippingAddressesByUserIdAsync(Guid userId)
        {
            return await applicationDbContext.ShippingAddresses
                                             .Where((shippingAddresses) => shippingAddresses.UserId == userId)
                                             .ToListAsync();
        }

        public async Task CreateShippingAddressAsync(ShippingAddress shippingAddress)
        {
            await applicationDbContext.ShippingAddresses.AddAsync(shippingAddress);
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateShippingAddressAsync(ShippingAddress shippingAddress)
        {
            applicationDbContext.ShippingAddresses.Update(shippingAddress);
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task DeleteShippingAddressAsync(ShippingAddress shippingAddress)
        {
            applicationDbContext.ShippingAddresses.Remove(shippingAddress);
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task<ShippingAddress?> GetPrimaryShippingAddressAsync() 
        {
            return await applicationDbContext.ShippingAddresses
                                             .FirstOrDefaultAsync((shippingAddresses) => shippingAddresses.IsPrimary == true);
        }
    }
}
