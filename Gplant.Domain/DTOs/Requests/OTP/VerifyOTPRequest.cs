namespace Gplant.Domain.DTOs.Requests.OTP
{
    public record VerifyOTPRequest
    {
        public required string Email { get; init; }
        public required string Code { get; init; }
    }
}
