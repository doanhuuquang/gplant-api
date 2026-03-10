using Gplant.Domain.Enums;

namespace Gplant.Domain.DTOs.Requests.Order
{
    public record UpdateOrderStatusRequest
    {
        public required OrderStatus Status { get; init; }
        public string? Note { get; init; }
    }
}