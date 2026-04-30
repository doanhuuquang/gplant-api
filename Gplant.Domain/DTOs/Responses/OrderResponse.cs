using Gplant.Domain.Enums;

namespace Gplant.Domain.DTOs.Responses
{
    public record OrderResponse
    {
        public Guid Id { get; init; }
        public string OrderNumber { get; init; } = string.Empty;
        public required UserResponse User { get; init; }
        public string ShippingName { get; init; } = string.Empty;
        public string ShippingPhone { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string BuildingName { get; init; } = string.Empty;
        public string Longitude { get; init; } = string.Empty;
        public string Latitude { get; init; } = string.Empty;
        public string? ShippingNote { get; init; }
        public decimal SubTotal { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal ShippingFee { get; init; }
        public decimal Total { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public string PaymentMethodDisplay => PaymentMethod.ToString();
        public PaymentStatus PaymentStatus { get; init; }
        public string PaymentStatusDisplay => PaymentStatus.ToString();
        public DateTimeOffset? PaidAtUtc { get; init; }
        public OrderStatus Status { get; init; }
        public string StatusDisplay => Status.ToString();
        public string? CancellationReason { get; init; }
        public DateTimeOffset? CancelledAtUtc { get; init; }
        public List<OrderItemResponse> Items { get; init; } = [];
        public int TotalItems => Items.Sum(i => i.Quantity);
        public bool CanCancel => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}