using System.ComponentModel.DataAnnotations;

namespace Gplant.Domain.DTOs.Requests
{
    public record CreateCategoryRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string ImageUrl { get; init; }
        public Guid? ParentId { get; init; }
    }
}
