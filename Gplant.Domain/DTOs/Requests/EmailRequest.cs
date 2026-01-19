namespace Gplant.Domain.DTOs.Requests
{
    public record EmailRequest
    {
        public required string Receptor { get; init; }
        public required string Subject { get; init; }
        public required string Body { get; init; }
    }
}
