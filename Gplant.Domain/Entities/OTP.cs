namespace Gplant.Domain.Entities
{
    public class OTP
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string Email { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Boolean IsUsed { get; set; }
    }
}
