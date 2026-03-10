using Gplant.Domain.Entities;

namespace Gplant.Domain.DTOs.Responses
{
    public record PlantImageResponse
    {
        public Guid Id { get; set; }
        public Guid PlantId { get; set; }
        public Media? Media { get; set; }
        public bool IsPrimary { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}