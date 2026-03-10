namespace Gplant.Domain.DTOs.Requests.Plant
{
    public record UpdatePlantRequest
    {
        public string? Name { get; init; }
        public string? ShortDescription { get; init; }
        public string? Description { get; init; }
        public Guid? CategoryId { get; init; }
        public Guid? CareInstructionId { get; init; }
        public bool? IsActive { get; init; }
    }
}