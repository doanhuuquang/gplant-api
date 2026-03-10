namespace Gplant.Domain.DTOs.Requests.PlantVariant
{
    public record CreatePlantVariantRequest
    {
        public required Guid PlantId { get; init; }
        public required string SKU { get; init; }
        public required decimal Price { get; init; }
        public required float Size { get; init; }
        public bool IsActive { get; init; } = true;
    }
}