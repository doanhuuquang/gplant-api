namespace Gplant.Domain.DTOs.Responses
{
    public record CreateOrderResponse
    {
        public OrderResponse Order { get; init; } = null!;
        public string? PaymentUrl { get; init; } 
        public string? QrCodeBase64 { get; init; }  
        public bool RequiresPayment { get; init; }
        public DateTimeOffset? PaymentExpireAtUtc { get; init; }
    }
}