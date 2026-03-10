using Gplant.Domain.enums;

namespace Gplant.Domain.DTOs.Requests.Banner
{
    public record UpdateBannerRequest
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public string? RedirectUrl { get; init; }
        public Guid? MediaId { get; init; }
        public BannerGroup? Group { get; init; }
        public int? OrderIndex { get; init; }
        public bool? IsActive { get; init; }
    }
}