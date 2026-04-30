using Gplant.Domain.Enums;

namespace Gplant.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }

        public string? GatewayTransactionId { get; set; }  // ID từ VNPay / bank
        public string? GatewayName { get; set; }           // "VNPay", "BankTransfer", "COD"
        public string? FailureReason { get; set; }         // Lý do thất bại
        public string? IpAddress { get; set; }             // IP user khi thanh toán
        public string? GatewayResponse { get; set; }       // Raw JSON response

        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public DateTimeOffset? PaidAtUtc { get; set; }

        public Order Order { get; set; } = null!;
    }
}
