using Gplant.Domain.Entities;

namespace Gplant.Domain.DTOs.Responses
{
    public record CategoryResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Media? Media { get; init; } 
        public Guid? ParentId { get; init; }
        public bool IsActive { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}