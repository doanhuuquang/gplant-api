namespace Gplant.Domain.DTOs.Requests
{
    public record ResetPasswordRequest
    {
        public required string Email { get; init; }
        public required string ResetPasswordToken { get; init; }
        public required string NewPassword { get; init; }
    }
}
