namespace Gplant.Domain.DTOs.Requests.Payment
{
    public record PaymentCallbackRequest
    {
        public string? OrderId { get; init; }
        public string? TransactionId { get; init; }
        public string? ResponseCode { get; init; }
        public string? Message { get; init; }
        public Dictionary<string, string>? AdditionalData { get; init; }
    }
}