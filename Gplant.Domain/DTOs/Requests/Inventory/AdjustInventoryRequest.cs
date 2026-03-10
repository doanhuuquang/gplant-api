namespace Gplant.Domain.DTOs.Requests.Inventory
{
    public record AdjustInventoryRequest
    {
        public required int Quantity { get; init; }
        public string? Reason { get; init; }
    }
}