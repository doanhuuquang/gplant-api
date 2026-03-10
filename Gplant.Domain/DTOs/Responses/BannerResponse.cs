using Gplant.Domain.Entities;
using Gplant.Domain.enums;

namespace Gplant.Domain.DTOs.Responses
{
    public record BannerResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public Media? Media{ get; set; }
        public required string RedirectUrl { get; set; }
        public required BannerGroup Group { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}