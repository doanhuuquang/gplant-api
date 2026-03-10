namespace Gplant.Domain.Entities
{
    public class PlantImage
    {
        public Guid Id { get; set; }
        public Guid PlantId { get; set; }
        public Guid? MediaId { get; set; }
        public bool IsPrimary { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}
