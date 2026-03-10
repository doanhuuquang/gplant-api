namespace Gplant.Domain.DTOs.Requests.Auth
{
    public record RecoverUsernameRequest
    {
        public required string Email { get; init; }
    }
}
