namespace Gplant.Domain.DTOs.Responses
{
    public record PlantResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public string ShortDescription { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public CategoryResponse Category { get; init; } = new CategoryResponse();
        public CareInstructionResponse? CareInstruction { get; init; }
        public List<PlantVariantResponse> Variants { get; init; } = [];
        public List<PlantImageResponse> Images { get; init; } = [];
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public bool IsActive { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}