namespace Gplant.Domain.DTOs.Requests.Inventory
{
    public record CreateInventoryRequest
    {
        public required Guid PlantVariantId { get; init; }
        public int QuantityAvailable { get; init; } = 0;
    }
}