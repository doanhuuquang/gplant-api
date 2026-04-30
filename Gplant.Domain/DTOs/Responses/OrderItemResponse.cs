namespace Gplant.Domain.DTOs.Responses
{
    public record OrderItemResponse
    {
        public Guid Id { get; init; }
        public Guid OrderId { get; init; }
        public Guid PlantVariantId { get; init; }
        public PlantResponse? Plant { get; init; }
        public string PlantName { get; init; } = string.Empty;
        public string VariantSKU { get; init; } = string.Empty;
        public float VariantSize { get; init; }
        public int Quantity { get; init; }
        public decimal Price { get; init; }
        public decimal? SalePrice { get; init; }
        public decimal FinalPrice { get; init; }
        public decimal SubTotal { get; init; }
        public decimal DiscountAmount => SalePrice.HasValue ? (Price - SalePrice.Value) * Quantity : 0;
        public bool WasOnSale => SalePrice.HasValue;
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}