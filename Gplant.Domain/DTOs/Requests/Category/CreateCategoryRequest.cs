namespace Gplant.Domain.DTOs.Requests.Category
{
    public record CreateCategoryRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public Guid? MediaId { get; init; } 
        public Guid? ParentId { get; init; }
    }
}
