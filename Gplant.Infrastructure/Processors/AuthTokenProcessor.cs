using Gplant.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Gplant.Application.Abstracts;
using Gplant.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gplant.Infrastructure.Processors
{
    public class AuthTokenProcessor(IOptions<JwtOptions> jwtOptions, IHttpContextAccessor httpContextAccessor) : IAuthTokenProcessor
    {
        public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user, IEnumerable<string> roles)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret));

            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

            var expires = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationTimeInMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Value.Issuer,
                audience: jwtOptions.Value.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );
            
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (jwtToken, expires);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            RandomNumberGenerator.Create().GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration)
        {
            httpContextAccessor?.HttpContext?.Response.Cookies.Append(
            cookieName,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Expires = expiration,
                IsEssential = true,
                Secure = true,
            }
        );
        }

        public void DeleteAuthTokenCookie(string cookieName)
        {
            httpContextAccessor?.HttpContext?.Response.Cookies.Delete(cookieName);
        }
    }
}
