using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    }
}
