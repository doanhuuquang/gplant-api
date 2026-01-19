namespace Gplant.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public Boolean IsActive { get; set; } = true;
    }
}
