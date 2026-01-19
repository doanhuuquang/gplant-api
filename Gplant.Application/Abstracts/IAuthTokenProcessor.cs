using Gplant.Domain.Entities;

namespace Gplant.Application.Abstracts
{
    public interface IAuthTokenProcessor
    {
        public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user, IEnumerable<string> roles);
        public string GenerateRefreshToken();
        public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration);
        public void DeleteAuthTokenCookie(string cookieName);
    }
}
