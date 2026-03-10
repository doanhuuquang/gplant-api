namespace Gplant.Domain.DTOs.Requests.Order
{
    public record CancelOrderRequest
    {
        public required string Reason { get; init; }
    }
}