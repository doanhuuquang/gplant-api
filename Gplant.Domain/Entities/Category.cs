namespace Gplant.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public required string Description { get; set; }
        public Guid? MediaId { get; set; }  
        public Guid? ParentId { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
