namespace Gplant.Domain.Entities
{
    public class LightningSaleItem
    {
        public Guid Id { get; set; }
        public Guid LightningSaleId { get; set; }
        public Guid PlantVariantId { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int QuantityLimit { get; set; }
        public int QuantitySold { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}