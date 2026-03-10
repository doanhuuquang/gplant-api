using Gplant.Domain.DTOs.Requests.Banner;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IBannerService
    {
        public Task<List<BannerResponse>> GetBannersAsync();
        public Task<List<BannerResponse>> GetActiveBannersAsync();
        public Task<BannerResponse?> GetBannerByOrderIndex(int orderIndex);
        public Task CreateBannerAsync(CreateBannerRequest createBannerRequest);
        public Task UpdateBannerAsync(Guid id, UpdateBannerRequest updateBannerRequest);
        public Task DeleteBannerAsync(Guid id);
        public Task ToggleActiveAsync(Guid id);
    }
}
