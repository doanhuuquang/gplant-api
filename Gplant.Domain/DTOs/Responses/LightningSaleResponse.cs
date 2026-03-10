using Gplant.Domain.Entities;

namespace Gplant.Domain.DTOs.Responses
{
    public record LightningSaleResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTimeOffset StartDateUtc { get; init; }
        public DateTimeOffset EndDateUtc { get; init; }
        public bool IsActive { get; init; }
        public bool IsUpcoming => DateTimeOffset.UtcNow < StartDateUtc;
        public bool IsOngoing => DateTimeOffset.UtcNow >= StartDateUtc && DateTimeOffset.UtcNow <= EndDateUtc;
        public bool IsExpired => DateTimeOffset.UtcNow > EndDateUtc;
        public TimeSpan? TimeRemaining => IsOngoing ? EndDateUtc - DateTimeOffset.UtcNow : null;
        public List<LightningSaleItemResponse> Items { get; init; } = [];
        public int TotalItems => Items.Count;
        public int ActiveItems => Items.Count(i => i.IsActive && !i.IsSoldOut);
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}