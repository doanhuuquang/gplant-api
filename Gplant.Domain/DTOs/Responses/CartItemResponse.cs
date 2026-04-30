namespace Gplant.Domain.DTOs.Responses
{
    public record CartItemResponse
    {
        public Guid Id { get; init; }
        public Guid CartId { get; init; }
        public Guid PlantVariantId { get; init; }
        public PlantVariantResponse? PlantVariant { get; init; }
        public PlantResponse? Plant { get; init; }
        public int Quantity { get; init; }
        public decimal Price { get; init; }
        public decimal? SalePrice { get; init; }
        public int DiscountedQuantity { get; init; } = 0;
        public decimal? DiscountPercentage => SalePrice.HasValue ? Math.Round((Price - SalePrice.Value) / Price * 100, 2) : null;
        public decimal FinalPrice => SalePrice ?? Price;
        public decimal TotalPrice =>
            SalePrice.HasValue
                ? (SalePrice.Value * DiscountedQuantity) + (Price * (Quantity - DiscountedQuantity))
                : Price * Quantity;
        public decimal DiscountAmount =>
            SalePrice.HasValue
                ? (Price - SalePrice.Value) * DiscountedQuantity
                : 0;
        public bool IsOnSale => SalePrice.HasValue && DiscountedQuantity > 0;
        public bool IsInStock { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}