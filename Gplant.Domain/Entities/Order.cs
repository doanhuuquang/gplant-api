using Gplant.Domain.Enums;

namespace Gplant.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public required string OrderNumber { get; set; }
        public Guid UserId { get; set; }
        
        // Shipping Info
        public required string ShippingName { get; set; }
        public required string ShippingPhone { get; set; }
        public required string Address { get; set; }
        public required string BuildingName { get; set; }
        public required string Longitude { get; set; }
        public required string Latitude { get; set; }
        public string? ShippingNote { get; set; }
        
        // Pricing
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        
        // Payment
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTimeOffset? PaidAtUtc { get; set; }
        
        // Order Status
        public OrderStatus Status { get; set; }
        public string? CancellationReason { get; set; }
        public DateTimeOffset? CancelledAtUtc { get; set; }
        
        // Payment tracking
        public string? PaymentTransactionId { get; set; }
        public string? PaymentGatewayResponse { get; set; }
        public string? VNPayTransactionId { get; set; }
        public DateTimeOffset? PaymentAttemptedAtUtc { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }

        public ICollection<Payment> Payments { get; set; } = [];
    }
}