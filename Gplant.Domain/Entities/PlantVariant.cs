namespace Gplant.Domain.Entities
{
    public class PlantVariant
    {
        public Guid Id { get; set; }
        public Guid PlantId { get; set; }
        public required string SKU { get; set; }
        public decimal Price { get; set; }
        public float Size { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}
