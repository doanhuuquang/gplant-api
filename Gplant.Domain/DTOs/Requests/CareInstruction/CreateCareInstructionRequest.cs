namespace Gplant.Domain.DTOs.Requests.CareInstruction
{
    public record CreateCareInstructionRequest
    {
        public required string LightRequirement { get; init; }
        public required string WateringFrequency { get; init; }
        public required string Temperature { get; init; }
        public required string Soil { get; init; }
        public required string Notes { get; init; }
    }
}