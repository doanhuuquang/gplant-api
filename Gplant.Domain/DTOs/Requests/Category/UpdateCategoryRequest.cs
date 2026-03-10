namespace Gplant.Domain.DTOs.Requests.Category
{
    public record UpdateCategoryRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public Guid? ParentId { get; init; }
        public Guid? MediaId { get; init; }
        public bool IsActive { get; init; } 
    }
}
