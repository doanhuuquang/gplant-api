namespace Gplant.Domain.Entities
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public Guid PlantVariantId { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public DateTimeOffset LastUpdatedAtUtc { get; set; }
    }
}
