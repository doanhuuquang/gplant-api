namespace Gplant.Domain.DTOs.Requests.PlantVariant
{
    public record UpdatePlantVariantRequest
    {
        public string? SKU { get; init; }
        public decimal? Price { get; init; }
        public float? Size { get; init; }
        public bool? IsActive { get; init; }
    }
}