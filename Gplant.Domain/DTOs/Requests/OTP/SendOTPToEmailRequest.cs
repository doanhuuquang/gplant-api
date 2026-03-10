namespace Gplant.Domain.DTOs.Requests.OTP
{
    public record SendOTPToEmailRequest
    {
        public required string Email { get; init; }
    }
}
