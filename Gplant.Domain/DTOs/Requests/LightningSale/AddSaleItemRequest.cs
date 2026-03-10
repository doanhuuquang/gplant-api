namespace Gplant.Domain.DTOs.Requests.LightningSale
{
    public record AddSaleItemRequest
    {
        public required Guid PlantVariantId { get; init; }
        public required decimal SalePrice { get; init; }
        public required int QuantityLimit { get; init; }
    }
}