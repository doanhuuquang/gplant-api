using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Gplant.Infrastructure.Repositories
{
    public class BannerRepository(ApplicationDbContext applicationDbContext) : IBannerRepository
    {
        public async Task<List<Banner>> GetBannersAsync()
        {
            return await applicationDbContext.Banners
                        .AsNoTracking()
                        .ToListAsync();
        }

        public async Task<List<Banner>> GetActiveBannersAsync()
        {
            return await applicationDbContext.Banners
                        .AsNoTracking()
                        .Where(banner => banner.IsActive)
                        .ToListAsync();
        }

        public async Task<Banner?> GetBannerByIdAsync(Guid id)
        {
            return await applicationDbContext.Banners
                        .FirstOrDefaultAsync(banner => banner.Id == id);
        }

        public async Task<Banner?> GetBannerByOrderIndexAsync(int orderIndex)
        {
            return await applicationDbContext.Banners
                        .AsNoTracking()
                        .FirstOrDefaultAsync(banner => banner.OrderIndex == orderIndex);
        }

        public async Task CreateBannerAsync(Banner banner)
        {
            await applicationDbContext.Banners.AddAsync(banner);
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateBannerAsync(Banner banner)
        {
            applicationDbContext.Banners.Update(banner);
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task DeleteBannerAsync(Banner banner)
        {
            applicationDbContext.Banners.Remove(banner);
            await applicationDbContext.SaveChangesAsync();
        }
    }
}
