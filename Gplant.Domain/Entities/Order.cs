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
        public required string ShippingAddress { get; set; }
        public required string ShippingWard { get; set; }
        public required string ShippingDistrict { get; set; }
        public required string ShippingProvince { get; set; }
        public string? ShippingNote { get; set; }
        public string? ShippingEmail { get; set; }
        
        // Pricing
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        
        // Payment
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? PaidAtUtc { get; set; }
        
        // Order Status
        public OrderStatus Status { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        
        // Payment tracking
        public string? PaymentTransactionId { get; set; }
        public string? PaymentGatewayResponse { get; set; }
        public DateTime? PaymentAttemptedAtUtc { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}