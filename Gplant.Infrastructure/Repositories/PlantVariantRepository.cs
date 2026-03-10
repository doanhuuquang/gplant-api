using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class PlantVariantRepository(ApplicationDbContext context) : IPlantVariantRepository
    {
        public async Task<PlantVariant?> GetByIdAsync(Guid id)
        {
            return await context.PlantVariants.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<PlantVariant?> GetBySKUAsync(string sku)
        {
            return await context.PlantVariants.FirstOrDefaultAsync(v => v.SKU == sku);
        }

        public async Task<List<PlantVariant>> GetByPlantIdAsync(Guid plantId)
        {
            return await context.PlantVariants
                .Where(v => v.PlantId == plantId)
                .OrderBy(v => v.Size)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateAsync(PlantVariant variant)
        {
            await context.PlantVariants.AddAsync(variant);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlantVariant variant)
        {
            context.PlantVariants.Update(variant);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PlantVariant variant)
        {
            context.PlantVariants.Remove(variant);
            await context.SaveChangesAsync();
        }

        public async Task<bool> SKUExistsAsync(string sku, Guid? excludeId = null)
        {
            return await context.PlantVariants
                .AnyAsync(v => v.SKU == sku && (!excludeId.HasValue || v.Id != excludeId.Value));
        }

        public async Task<bool> IsUsedInOrdersAsync(Guid variantId)
        {
            return await context.OrderItems
                .AnyAsync(oi => oi.PlantVariantId == variantId);
        }

        public async Task<bool> IsUsedInCartsAsync(Guid variantId)
        {
            return await context.CartItems
                .AnyAsync(ci => ci.PlantVariantId == variantId);
        }

        public async Task<bool> HasInventoryAsync(Guid variantId)
        {
            return await context.Inventories
                .AnyAsync(i => i.PlantVariantId == variantId);
        }
    }
}