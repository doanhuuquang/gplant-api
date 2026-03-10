using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class LightningSaleRepository(ApplicationDbContext context) : ILightningSaleRepository
    {
        public async Task<LightningSale?> GetByIdAsync(Guid id)
        {
            return await context.LightningSales.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<LightningSale>> GetAllAsync()
        {
            return await context.LightningSales
                .OrderByDescending(s => s.StartDateUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<LightningSale>> GetActiveAsync()
        {
            return await context.LightningSales
                .Where(s => s.IsActive)
                .OrderBy(s => s.StartDateUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<LightningSale>> GetUpcomingAsync()
        {
            var now = DateTime.UtcNow;
            return await context.LightningSales
                .Where(s => s.StartDateUtc > now)
                .OrderBy(s => s.StartDateUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<LightningSale>> GetOngoingAsync()
        {
            var now = DateTime.UtcNow;
            return await context.LightningSales
                .Where(s => s.IsActive && s.StartDateUtc <= now && s.EndDateUtc >= now)
                .OrderBy(s => s.EndDateUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LightningSale?> GetCurrentActiveSaleAsync()
        {
            var now = DateTime.UtcNow;
            return await context.LightningSales
                .Where(s => s.IsActive && s.StartDateUtc <= now && s.EndDateUtc >= now)
                .OrderBy(s => s.EndDateUtc)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(LightningSale sale)
        {
            await context.LightningSales.AddAsync(sale);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LightningSale sale)
        {
            sale.UpdatedAtUtc = DateTime.UtcNow;
            context.LightningSales.Update(sale);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LightningSale sale)
        {
            context.LightningSales.Remove(sale);
            await context.SaveChangesAsync();
        }
    }
}