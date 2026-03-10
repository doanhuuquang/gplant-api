namespace Gplant.Domain.DTOs.Requests.LightningSale
{
    public record UpdateLightningSaleRequest
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public DateTimeOffset? StartDateUtc { get; init; }
        public DateTimeOffset? EndDateUtc { get; init; }
        public bool? IsActive { get; init; }
    }
}