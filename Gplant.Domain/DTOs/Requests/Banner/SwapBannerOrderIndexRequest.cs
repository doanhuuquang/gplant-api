namespace Gplant.Domain.DTOs.Requests.Banner
{
    public record SwapBannerOrderIndexRequest
    {
        public required Guid BannerId1 { get; init; }
        public required Guid BannerId2 { get; init; }
    }
}
