using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Banner;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Banner;

namespace Gplant.Application.Services
{
    public class BannerService(IBannerRepository bannerRepository, IMediaRepository mediaRepository) : IBannerService
    {
        public async Task<List<BannerResponse>> GetBannersAsync()
        {
            var banners = await bannerRepository.GetBannersAsync();

            var responses = new List<BannerResponse>();
            foreach (var banner in banners)
            {
                responses.Add(await MapToResponseAsync(banner));
            }

            return responses;
        }

        public async Task<List<BannerResponse>> GetActiveBannersAsync()
        {
            var banners = await bannerRepository.GetActiveBannersAsync();

            var responses = new List<BannerResponse>();
            foreach (var banner in banners)
            {
                responses.Add(await MapToResponseAsync(banner));
            }

            return responses;
        }

        public async Task<BannerResponse?> GetBannerByOrderIndex(int orderIndex)
        {
            var banner = await bannerRepository.GetBannerByOrderIndexAsync(orderIndex) ?? throw new BannerNotFoundException("Banner not found");
            return await MapToResponseAsync(banner);
        }

        public async Task CreateBannerAsync(CreateBannerRequest createBannerRequest)
        {
            if (string.IsNullOrWhiteSpace(createBannerRequest.Title)) throw new CreateBannerException("Banner title is required");
            if (string.IsNullOrWhiteSpace(createBannerRequest.Description)) throw new CreateBannerException("Banner description is required");
            if (string.IsNullOrWhiteSpace(createBannerRequest.RedirectUrl)) throw new CreateBannerException("Redirect url is required");
            if (!Enum.IsDefined(createBannerRequest.Group)) throw new CreateBannerException("Banner group is required or not found");

            if (createBannerRequest.MediaId.HasValue)
            {
                _ = await mediaRepository.GetByIdAsync(createBannerRequest.MediaId.Value) ?? throw new CreateBannerException("Media does not exist");
            }

            // ✅ Lấy banner CÙNG GROUP
            var allBanners = await bannerRepository.GetBannersAsync();
            var bannersInSameGroup = allBanners
                                    .Where(b => b.Group == createBannerRequest.Group)
                                    .ToList();

            int orderIndex;

            if (createBannerRequest.OrderIndex == null || createBannerRequest.OrderIndex <= 0)
            {
                // ✅ Tự động gán = số lượng banner trong group + 1
                orderIndex = bannersInSameGroup.Count + 1;
            }
            else
            {
                orderIndex = createBannerRequest.OrderIndex.Value;

                // ✅ Chỉ đẩy OrderIndex của banner CÙNG GROUP
                var affectedBanners = bannersInSameGroup.Where(b => b.OrderIndex >= orderIndex)
                                                        .OrderByDescending(b => b.OrderIndex)
                                                        .ToList();

                foreach (var banner in affectedBanners)
                {
                    banner.OrderIndex += 1;
                    await bannerRepository.UpdateBannerAsync(banner);
                }
            }

            var newBanner = new Banner
            {
                Title = createBannerRequest.Title,
                Description = createBannerRequest.Description,
                MediaId = createBannerRequest.MediaId,
                RedirectUrl = createBannerRequest.RedirectUrl,
                Group = createBannerRequest.Group,
                OrderIndex = orderIndex,
                IsActive = createBannerRequest.IsActive,
            };

            await bannerRepository.CreateBannerAsync(newBanner);
        }

        public async Task UpdateBannerAsync(Guid id, UpdateBannerRequest updateBannerRequest)
        {
            var banner = await bannerRepository.GetBannerByIdAsync(id) ?? throw new BannerNotFoundException($"Banner with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(updateBannerRequest.Title)) banner.Title = updateBannerRequest.Title;
            if (!string.IsNullOrWhiteSpace(updateBannerRequest.Description)) banner.Description = updateBannerRequest.Description;
            if (!string.IsNullOrWhiteSpace(updateBannerRequest.RedirectUrl)) banner.RedirectUrl = updateBannerRequest.RedirectUrl;
            if (updateBannerRequest.IsActive.HasValue) banner.IsActive = updateBannerRequest.IsActive.Value;

            if (updateBannerRequest.MediaId.HasValue && updateBannerRequest.MediaId != banner.MediaId)
            {
                _ = await mediaRepository.GetByIdAsync(updateBannerRequest.MediaId.Value) ?? throw new UpdateBannerException("Media does not exist");
                banner.MediaId = updateBannerRequest.MediaId;
            }

            // ✅ Xử lý thay đổi Group
            if (updateBannerRequest.Group.HasValue && updateBannerRequest.Group.Value != banner.Group)
            {
                if (!Enum.IsDefined(updateBannerRequest.Group.Value)) throw new UpdateBannerException("Invalid banner group");

                var oldGroup = banner.Group;
                var newGroup = updateBannerRequest.Group.Value;
                var oldOrderIndex = banner.OrderIndex;

                // Lấy banners của group cũ
                var oldGroupBanners = (await bannerRepository.GetBannersAsync())
                                                             .Where(b => b.Id != id && b.Group == oldGroup && b.OrderIndex > oldOrderIndex)
                                                             .ToList();

                // Đẩy OrderIndex của group cũ xuống
                foreach (var b in oldGroupBanners)
                {
                    b.OrderIndex -= 1;
                    await bannerRepository.UpdateBannerAsync(b);
                }

                // Lấy banners của group mới
                var newGroupBanners = (await bannerRepository.GetBannersAsync())
                                                             .Where(b => b.Group == newGroup)
                                                             .ToList();

                // Gán OrderIndex cuối cùng trong group mới
                banner.Group = newGroup;
                banner.OrderIndex = newGroupBanners.Count + 1;
            }

            // ✅ Xử lý thay đổi OrderIndex (TRONG CÙNG GROUP)
            if (updateBannerRequest.OrderIndex.HasValue && updateBannerRequest.OrderIndex.Value != banner.OrderIndex)
            {
                int oldOrderIndex = banner.OrderIndex;
                int newOrderIndex = updateBannerRequest.OrderIndex.Value;

                if (newOrderIndex <= 0) throw new UpdateBannerException("OrderIndex must be greater than 0");

                // ✅ Chỉ lấy banner CÙNG GROUP
                var allBanners = await bannerRepository.GetBannersAsync();
                var bannersInSameGroup = allBanners.Where(b => b.Id != id && b.Group == banner.Group)
                                                   .ToList();

                if (newOrderIndex > oldOrderIndex)
                {
                    // Di chuyển lên (VD: từ vị trí 2 → 5)
                    var affectedBanners = bannersInSameGroup.Where(b => b.OrderIndex > oldOrderIndex && b.OrderIndex <= newOrderIndex)
                                                            .ToList();

                    foreach (var affected in affectedBanners)
                    {
                        affected.OrderIndex -= 1;
                        await bannerRepository.UpdateBannerAsync(affected);
                    }
                }
                else
                {
                    // Di chuyển xuống (VD: từ vị trí 5 → 2)
                    var affectedBanners = bannersInSameGroup.Where(b => b.OrderIndex >= newOrderIndex && b.OrderIndex < oldOrderIndex)
                                                            .ToList();

                    foreach (var affected in affectedBanners)
                    {
                        affected.OrderIndex += 1;
                        await bannerRepository.UpdateBannerAsync(affected);
                    }
                }

                banner.OrderIndex = newOrderIndex;
            }

            banner.UpdatedAtUtc = DateTime.UtcNow;
            await bannerRepository.UpdateBannerAsync(banner);
        }

        public async Task DeleteBannerAsync(Guid id)
        {
            var banner = await bannerRepository.GetBannerByIdAsync(id) ?? throw new BannerNotFoundException($"Banner with ID {id} not found");
            var allBanners = await bannerRepository.GetBannersAsync();
            
            // ✅ Chỉ cập nhật OrderIndex của banner CÙNG GROUP
            var bannersToUpdate = allBanners.Where(b => b.Group == banner.Group && b.OrderIndex > banner.OrderIndex)
                                            .OrderBy(b => b.OrderIndex)
                                            .ToList();

            await bannerRepository.DeleteBannerAsync(banner);

            foreach (var b in bannersToUpdate)
            {
                b.OrderIndex -= 1;
                await bannerRepository.UpdateBannerAsync(b);
            }
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var banner = await bannerRepository.GetBannerByIdAsync(id) ?? throw new BannerNotFoundException($"Banner with ID {id} not found");

            banner.IsActive = !banner.IsActive;
            banner.UpdatedAtUtc = DateTime.UtcNow;

            await bannerRepository.UpdateBannerAsync(banner);
        }

        private async Task<BannerResponse> MapToResponseAsync(Banner banner)
        {
            Media? media = null;

            if (banner.MediaId.HasValue)
            {
                media = await mediaRepository.GetByIdAsync(banner.MediaId.Value);
            }

            return new BannerResponse
            {
                Id = banner.Id,
                Title = banner.Title,
                Description = banner.Description,
                Media = media,
                RedirectUrl = banner.RedirectUrl,
                Group = banner.Group,
                OrderIndex = banner.OrderIndex,
                IsActive = banner.IsActive,
                CreatedAtUtc = banner.CreatedAtUtc,
                UpdatedAtUtc = banner.UpdatedAtUtc,
            };
        }
    }
}
