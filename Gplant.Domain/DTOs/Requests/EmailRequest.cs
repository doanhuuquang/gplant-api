namespace Gplant.Domain.DTOs.Requests
{
    public record EmailRequest
    {
        public required string Receptor { get; init; }
        public required string subject { get; init; }
        public required string body { get; init; }
    }
}
