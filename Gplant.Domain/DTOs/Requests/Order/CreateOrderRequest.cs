using Gplant.Domain.Enums;

namespace Gplant.Domain.DTOs.Requests.Order
{
    public record CreateOrderRequest
    {
        public string ShippingName { get; init; } = string.Empty;
        public string ShippingPhone { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string BuildingName { get; init; } = string.Empty;
        public string Longitude { get; init; } = string.Empty;
        public string Latitude { get; init; } = string.Empty;
        public string? ShippingNote { get; init; }
        public PaymentMethod PaymentMethod { get; init; } = PaymentMethod.COD;
    }
}