namespace Gplant.Domain.DTOs.Requests
{
    public record SendOTPToEmailRequest
    {
        public required string Email { get; init; }
    }
}
