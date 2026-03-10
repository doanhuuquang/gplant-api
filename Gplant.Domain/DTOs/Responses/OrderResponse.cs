using Gplant.Domain.Enums;

namespace Gplant.Domain.DTOs.Responses
{
    public record OrderResponse
    {
        public Guid Id { get; init; }
        public string OrderNumber { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string ShippingName { get; init; } = string.Empty;
        public string ShippingPhone { get; init; } = string.Empty;
        public string ShippingAddress { get; init; } = string.Empty;
        public string ShippingWard { get; init; } = string.Empty;
        public string ShippingDistrict { get; init; } = string.Empty;
        public string ShippingProvince { get; init; } = string.Empty;
        public string? ShippingNote { get; init; }
        public string FullShippingAddress => $"{ShippingAddress}, {ShippingWard}, {ShippingDistrict}, {ShippingProvince}";
        public decimal SubTotal { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal ShippingFee { get; init; }
        public decimal Total { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public string PaymentMethodDisplay => PaymentMethod.ToString();
        public PaymentStatus PaymentStatus { get; init; }
        public string PaymentStatusDisplay => PaymentStatus.ToString();
        public DateTime? PaidAtUtc { get; init; }
        public OrderStatus Status { get; init; }
        public string StatusDisplay => Status.ToString();
        public string? CancellationReason { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public List<OrderItemResponse> Items { get; init; } = new();
        public int TotalItems => Items.Sum(i => i.Quantity);
        public bool CanCancel => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
        public string? ShippingEmail { get; set; }
    }
}