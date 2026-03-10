using Gplant.Domain.Enums;

namespace Gplant.Domain.DTOs.Requests.Order
{
    public record CreateOrderRequest
    {
        public required string ShippingName { get; init; }
        public required string ShippingPhone { get; init; }
        public required string ShippingAddress { get; init; }
        public required string ShippingWard { get; init; }
        public required string ShippingDistrict { get; init; }
        public required string ShippingProvince { get; init; }
        public string? ShippingNote { get; init; }
        public PaymentMethod PaymentMethod { get; init; } = PaymentMethod.COD;
        public string? CouponCode { get; init; }
        public string? ShippingEmail { get; set; }
    }
}