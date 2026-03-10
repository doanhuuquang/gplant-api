namespace Gplant.Domain.DTOs.Requests.LightningSale
{
    public record CreateLightningSaleRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required DateTime StartDateUtc { get; init; }
        public required DateTime EndDateUtc { get; init; }
    }
}