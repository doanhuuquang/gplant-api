using Gplant.Domain.Enums;

namespace Gplant.Domain.DTOs.Responses
{
    public record PaymentCallbackResponse
    {
        public bool Success { get; init; }
        public string OrderNumber { get; init; } = string.Empty;
        public string TransactionId { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
    }
}