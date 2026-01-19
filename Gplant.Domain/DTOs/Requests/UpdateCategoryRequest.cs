namespace Gplant.Domain.DTOs.Requests
{
    public record UpdateCategoryRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string ImageUrl { get; init; }
        public Guid? ParentId { get; init; }
        public Boolean IsActive { get; init; } 
    }
}
