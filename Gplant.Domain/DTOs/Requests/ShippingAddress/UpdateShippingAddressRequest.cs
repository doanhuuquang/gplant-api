namespace Gplant.Domain.DTOs.Requests.ShippingAddress
{
    public record UpdateShippingAddressRequest
    {
        public string? ShippingName { get; init; }
        public string? ShippingPhone { get; init; }
        public string? Address { get; init; }
        public string? BuildingName { get; init; }
        public bool IsPrimary { get; init; }
        public required string Longitude { get; init; }
        public required string Latitude { get; init; }
    }
}
