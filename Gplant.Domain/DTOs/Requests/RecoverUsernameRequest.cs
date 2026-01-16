namespace Gplant.Domain.DTOs.Requests
{
    public record RecoverUsernameRequest
    {
        public required string Email { get; init; }
    }
}
