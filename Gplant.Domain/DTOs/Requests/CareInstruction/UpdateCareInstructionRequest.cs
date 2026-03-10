namespace Gplant.Domain.DTOs.Requests.CareInstruction
{
    public record UpdateCareInstructionRequest
    {
        public string? LightRequirement { get; init; }
        public string? WateringFrequency { get; init; }
        public string? Temperature { get; init; }
        public string? Soil { get; init; }
        public string? Notes { get; init; }
    }
}