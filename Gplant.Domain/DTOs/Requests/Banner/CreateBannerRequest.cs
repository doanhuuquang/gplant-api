using Gplant.Domain.enums;

namespace Gplant.Domain.DTOs.Requests.Banner
{
    public record CreateBannerRequest
    {
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string ImageUrl { get; init; }
        public required string RedirectUrl { get; init; }
        public required BannerGroup Group { get; init; }
        public int? OrderIndex { get; init; }
        public Guid? MediaId { get; init; }
        public bool IsActive { get; init; }
    }
}
