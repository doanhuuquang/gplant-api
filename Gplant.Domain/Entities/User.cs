using Microsoft.AspNetCore.Identity;

namespace Gplant.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAtUtc { get; set; }

        public static User Create(string email)
        {
            return new User
            {
                UserName    = email,
                Email       = email,
            };
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
