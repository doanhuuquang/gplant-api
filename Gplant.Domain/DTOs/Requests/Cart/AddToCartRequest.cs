namespace Gplant.Domain.DTOs.Requests.Cart
{
    public record AddToCartRequest
    {
        public required Guid PlantVariantId { get; init; }
        public int Quantity { get; init; } = 1;
    }
}