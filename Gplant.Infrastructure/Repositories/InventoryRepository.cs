using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class InventoryRepository(ApplicationDbContext context) : IInventoryRepository
    {
        public async Task<Inventory?> GetByIdAsync(Guid id)
        {
            return await context.Inventories
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Inventory?> GetByPlantVariantIdAsync(Guid plantVariantId)
        {
            return await context.Inventories
                .FirstOrDefaultAsync(i => i.PlantVariantId == plantVariantId);
        }

        public async Task<List<Inventory>> GetAllAsync()
        {
            return await context.Inventories
                .AsNoTracking()
                .OrderBy(i => i.QuantityAvailable)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetLowStockItemsAsync(int threshold = 10)
        {
            return await context.Inventories
                .Where(i => i.QuantityAvailable > 0 && i.QuantityAvailable <= threshold)
                .AsNoTracking()
                .OrderBy(i => i.QuantityAvailable)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetOutOfStockItemsAsync()
        {
            return await context.Inventories
                .Where(i => i.QuantityAvailable == 0)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateAsync(Inventory inventory)
        {
            await context.Inventories.AddAsync(inventory);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            inventory.LastUpdatedAtUtc = DateTime.UtcNow;
            context.Inventories.Update(inventory);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Inventory inventory)
        {
            context.Inventories.Remove(inventory);
            await context.SaveChangesAsync();
        }
    }
}