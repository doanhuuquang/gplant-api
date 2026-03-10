namespace Gplant.Domain.DTOs.Requests.Plant
{
    public record PlantFilterRequest
    {
        public string? SearchTerm { get; init; }
        public Guid? CategoryId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public bool? IsActive { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? SortBy { get; init; } = "Name";
        public string? SortOrder { get; init; } = "asc";
    }
}