namespace Gplant.Domain.DTOs.Requests.Plant
{
    public record CreatePlantRequest
    {
        public required string Name { get; init; }
        public required string ShortDescription { get; init; }
        public required string Description { get; init; }
        public required Guid CategoryId { get; init; }
        public required Guid CareInstructionId { get; init; }
        public bool IsActive { get; init; } = true;
    }
}