using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class LightningSaleItemRepository(ApplicationDbContext context) : ILightningSaleItemRepository
    {
        public async Task<LightningSaleItem?> GetByIdAsync(Guid id)
        {
            return await context.LightningSaleItems.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<LightningSaleItem>> GetBySaleIdAsync(Guid saleId)
        {
            return await context.LightningSaleItems
                .Where(i => i.LightningSaleId == saleId)
                .OrderBy(i => i.CreatedAtUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LightningSaleItem?> GetByVariantIdAsync(Guid plantVariantId, Guid? saleId = null)
        {
            var now = DateTime.UtcNow;
            var query = context.LightningSaleItems
                .Where(i => i.PlantVariantId == plantVariantId && i.IsActive);

            if (saleId.HasValue)
            {
                query = query.Where(i => i.LightningSaleId == saleId.Value);
            }
            else
            {
                // Get active sale item from ongoing sales
                query = query.Where(i => 
                    context.LightningSales.Any(s => 
                        s.Id == i.LightningSaleId && 
                        s.IsActive && 
                        s.StartDateUtc <= now && 
                        s.EndDateUtc >= now));
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<LightningSaleItem>> GetActiveItemsAsync()
        {
            var now = DateTime.UtcNow;
            return await context.LightningSaleItems
                .Where(i => i.IsActive && 
                    context.LightningSales.Any(s => 
                        s.Id == i.LightningSaleId && 
                        s.IsActive && 
                        s.StartDateUtc <= now && 
                        s.EndDateUtc >= now))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateAsync(LightningSaleItem item)
        {
            await context.LightningSaleItems.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LightningSaleItem item)
        {
            item.UpdatedAtUtc = DateTime.UtcNow;
            context.LightningSaleItems.Update(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LightningSaleItem item)
        {
            context.LightningSaleItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }
}