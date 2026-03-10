namespace Gplant.Domain.DTOs.Requests.Inventory
{
    public record UpdateInventoryRequest
    {
        public int? QuantityAvailable { get; init; }
    }
}