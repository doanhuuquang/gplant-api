using Gplant.Domain.enums;

namespace Gplant.Domain.Entities
{
    public class ActionToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Token { get; set; }
        public ActionTokenPurpose Purpose { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Boolean IsUsed { get; set; }
    }
}
