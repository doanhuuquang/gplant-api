namespace Gplant.Domain.Entities
{
    public class ShippingAddress
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Address { get; set; }
        public required string BuildingName { get; set; }
        public required string ShippingName { get; set; }
        public required string ShippingPhone { get; set; }
        public required bool IsPrimary { get; set; }
        public required string Longitude { get; set; }
        public required string Latitude { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}
