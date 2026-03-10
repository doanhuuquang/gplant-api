using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IBannerRepository
    {
        public Task<List<Banner>> GetBannersAsync();
        public Task<List<Banner>> GetActiveBannersAsync();
        public Task<Banner?> GetBannerByIdAsync(Guid id);
        public Task<Banner?> GetBannerByOrderIndexAsync(int orderIndex);
        public Task CreateBannerAsync(Banner banner);
        public Task UpdateBannerAsync(Banner banner);
        public Task DeleteBannerAsync(Banner banner);
    }
}
