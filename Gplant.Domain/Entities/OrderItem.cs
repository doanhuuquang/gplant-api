namespace Gplant.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid PlantId { get; set; }
        public Guid PlantVariantId { get; set; }
        public required string PlantName { get; set; }
        public required string VariantSKU { get; set; }
        public float VariantSize { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal SubTotal { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}