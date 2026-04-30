namespace Gplant.Domain.DTOs.Requests.ShippingAddress
{
    public record AddShippingAddressRequest
    {
        public required string ShippingName { get; init; }
        public required string ShippingPhone { get; init; }
        public required string Address { get; init; }
        public required string BuildingName { get; init; }
        public bool IsPrimary { get; init; }
        public required string Longitude { get; init; }
        public required string Latitude { get; init; }

    }
}
