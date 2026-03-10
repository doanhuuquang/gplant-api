namespace Gplant.Domain.Entities
{
    public class Plant
    {
        public Guid Id { get; set; }
        public Guid CareInstructionId { get; set; }
        public Guid CategoryId { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public required string ShortDescription { get; set; }
        public required string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}
