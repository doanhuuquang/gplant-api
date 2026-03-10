namespace Gplant.Domain.Entities
{
    public class CareInstruction
    {
        public Guid Id { get; set; }
        public required string LightRequirement { get; set; }
        public required string WateringFrequency { get; set; }
        public required string Temperature { get; set; }
        public required string Soil { get; set; }
        public required string Notes { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}
