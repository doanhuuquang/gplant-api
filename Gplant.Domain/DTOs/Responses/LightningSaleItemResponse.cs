using Gplant.Domain.Entities;

namespace Gplant.Domain.DTOs.Responses
{
    public record LightningSaleItemResponse
    {
        public Guid Id { get; init; }
        public Guid LightningSaleId { get; init; }
        public PlantResponse? Plant { get; init; }
        public PlantVariantResponse? SalePlantVariant { get; init; }
        public decimal OriginalPrice { get; init; }
        public decimal SalePrice { get; init; }
        public decimal DiscountPercentage { get; init; }
        public decimal SavedAmount => OriginalPrice - SalePrice;
        public int QuantityLimit { get; init; }
        public int QuantitySold { get; init; }
        public int QuantityRemaining => QuantityLimit - QuantitySold;
        public decimal SoldPercentage => QuantityLimit > 0 ? (decimal)QuantitySold / QuantityLimit * 100 : 0;
        public bool IsSoldOut => QuantitySold >= QuantityLimit;
        public bool IsActive { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}