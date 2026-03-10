namespace Gplant.Domain.DTOs.Requests.Inventory
{
    public record ReserveInventoryRequest
    {
        public required Guid PlantVariantId { get; init; }
        public required int Quantity { get; init; }
    }
}