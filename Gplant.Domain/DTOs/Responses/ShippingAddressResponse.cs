namespace Gplant.Domain.DTOs.Responses
{
    public record ShippingAddressResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public required string ShippingName { get; init; }
        public required string ShippingPhone { get; init; }
        public required string Address { get; init; }
        public required string BuildingName { get; init; }
        public bool IsPrimary { get; init; }
        public required string Longitude { get; init; }
        public required string Latitude { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}
