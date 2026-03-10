namespace Gplant.Domain.DTOs.Requests.PlantImage
{
    public record CreatePlantImageRequest
    {
        public required Guid PlantId { get; init; }
        public Guid? MediaId{ get; init; }
        public bool IsPrimary { get; init; } = false;
    }
}