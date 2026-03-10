namespace Gplant.Domain.Entities
{
    public class LightningSale
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset StartDateUtc { get; set; }
        public DateTimeOffset EndDateUtc { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}