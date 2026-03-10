namespace Gplant.Domain.DTOs.Responses
{
    public record CreateOrderResponse
    {
        public OrderResponse Order { get; init; } = null!;
        public string? PaymentUrl { get; init; }
        public bool RequiresPayment { get; init; }
    }
}