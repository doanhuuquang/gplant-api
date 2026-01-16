namespace Gplant.Domain.DTOs.Requests
{
    public record VerifyOTPRequest
    {
        public required string Email { get; init; }
        public required string Code { get; init; }
    }
}
