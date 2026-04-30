using Gplant.Domain.Enums;

namespace Gplant.Domain.DTOs.Requests.Order
{
    public record OrderFilterRequest
    {
        public OrderStatus? Status { get; init; }
        public PaymentStatus? PaymentStatus { get; init; }
        public DateTimeOffset? FromDate { get; init; }
        public DateTimeOffset? ToDate { get; init; }
        public string? SearchTerm { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}