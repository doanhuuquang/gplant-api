namespace Gplant.Domain.DTOs.Requests.Cart
{
    public record UpdateCartItemRequest
    {
        public required int Quantity { get; init; }
    }
}