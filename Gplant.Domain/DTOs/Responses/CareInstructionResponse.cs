namespace Gplant.Domain.DTOs.Responses
{
    public record CareInstructionResponse
    {
        public Guid Id { get; init; }
        public required string LightRequirement { get; init; }
        public required string WateringFrequency { get; init; }
        public required string Temperature { get; init; }
        public required string Soil { get; init; }
        public required string Notes { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}
