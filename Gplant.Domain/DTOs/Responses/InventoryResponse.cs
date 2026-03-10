using Gplant.Domain.Entities;

namespace Gplant.Domain.DTOs.Responses
{
    public record InventoryResponse
    {
        public Guid Id { get; init; }
        public Guid PlantVariantId { get; init; }
        public PlantVariant? PlantVariant { get; init; }
        public int QuantityAvailable { get; init; }
        public int QuantityReserved { get; init; }
        public int TotalQuantity => QuantityAvailable + QuantityReserved;
        public bool IsInStock => QuantityAvailable > 0;
        public bool IsLowStock => QuantityAvailable > 0 && QuantityAvailable <= 10;
        public bool IsOutOfStock => QuantityAvailable == 0;
        public DateTimeOffset LastUpdatedAtUtc { get; init; }
    }
}